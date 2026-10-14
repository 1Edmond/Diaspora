using Marketplace.Application.Abstractions;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Mappings;
using Marketplace.Infrastructure.Consumers;
using Marketplace.Infrastructure.Extensions;
using Marketplace.Infrastructure.Persistence;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Seeders;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Runtime;

namespace Marketplace.Infrastructure;


public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        var connectionString = configuration.GetConnectionString("MarketplaceDb")
            ?? throw new InvalidOperationException("Marketplace database connection string is not configured.");

        services.AddScoped<IIntegrationEventMapper, MarketplaceIntegrationEventMapper>();

        services.AddScoped<IServiceListingRepository, ServiceListingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IProfileDirectoryRepository, ProfileDirectoryRepository>();
        services.AddScoped<Shared.Application.Abstractions.ITranslationRepository<Marketplace.Domain.Entities.Category>>(sp =>
            new Shared.Infrastructure.Persistence.Translations.TranslationRepository<Marketplace.Domain.Entities.Category>(
                sp.GetRequiredService<MarketplaceDbContext>(),
                sp.GetRequiredService<ILogger<Shared.Infrastructure.Persistence.Translations.TranslationRepository<Marketplace.Domain.Entities.Category>>>()));
        services.AddSingleton<CategorySeeder>();

        services.AddScoped<MarketplacePaymentHeldConsumer>();
        services.AddScoped<MarketplacePaymentFailedConsumer>();
        services.AddScoped<MarketplacePaymentReleasedConsumer>();
        services.AddScoped<MarketplacePaymentRefundedConsumer>();

        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureMarketplaceDomainQueues();

            var optimizedConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 60,
                CommandTimeout = 60,
                MaxPoolSize = 15,
                MinPoolSize = 0,
                Pooling = true,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 10
            }.ToString();

            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine_marketplace");

            options.ListenToRabbitQueue(ExchangesConstants.MarketplaceEventsExchange)
                .PreFetchCount(15)
                .UseDurableInbox();

            // Événements du Profile Service : alimentent la projection locale des profils
            // (correspondance UserId ↔ ProfileId, nom, pays, devise, statut de vérification).
            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.ProfileEventsExchange)
                .ToQueue("marketplace-service.profile-events");

            options.ListenToRabbitQueue("marketplace-service.profile-events")
                .PreFetchCount(20)
                .MaximumParallelMessages(3)
                .ListenerCount(2)
                .UseDurableInbox();

            // Événements du Wallet Service : notifications de séquestre (Held, Failed, Released, Refunded)
            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.WalletEventsExchange)
                .ToQueue("marketplace-service.wallet-events");

            options.ListenToRabbitQueue("marketplace-service.wallet-events")
                .PreFetchCount(20)
                .MaximumParallelMessages(3)
                .ListenerCount(2)
                .UseDurableInbox();

            // Événements du KYC Service : badge vérifié / preuve d'arrivée
            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.KycEventsExchange)
                .ToQueue("marketplace-service.kyc-events");

            options.ListenToRabbitQueue("marketplace-service.kyc-events")
                .PreFetchCount(20)
                .MaximumParallelMessages(3)
                .ListenerCount(2)
                .UseDurableInbox();

            options.UseEntityFrameworkCoreTransactions();
            options.Policies.AutoApplyTransactions();
            options.Discovery.IncludeAssembly(typeof(CreateCategoryCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(MarketplacePaymentHeldConsumer).Assembly);
            options.Discovery.IncludeAssembly(typeof(ProfileDirectoryConsumer).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        services.AddPostgresDatabase<MarketplaceDbContext>(connectionString, migrationsAssembly);
        services.AddScoped<IApplicationDbContext, MarketplaceDbContext>();
    }

    public static void BuildInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);
    }

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseCoreInfrastructure();

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        await context.Database.MigrateAsync();

        var seeder = new CategorySeeder(
            context,
            scope.ServiceProvider.GetRequiredService<ILogger<CategorySeeder>>()
        );
        await seeder.SeedAsync();
    }

    public static async Task PublishMarketplaceResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("MarketplaceService", typeof(T).Assembly);
       
        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Registered {Count} permissions",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}