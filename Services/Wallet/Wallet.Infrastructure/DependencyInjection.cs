using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Domain.Interfaces;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wallet.Application;
using Wallet.Application.Handlers.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Mappings;
using Wallet.Infrastructure.Consumers;
using Wallet.Infrastructure.Extensions;
using Wallet.Infrastructure.Persistences;
using Wallet.Infrastructure.Repositories;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Runtime;

namespace Wallet.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        services.AddWalletApplication();

        var connectionString = configuration.GetConnectionString("WalletDb")
            ?? throw new InvalidOperationException(
                "Wallet database connection string is not configured.");

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IWalletParameterRepository, WalletParameterRepository>();

        // ── UnitOfWork ────────────────────────────────────────────────────────
        //services.AddScoped<IUnitOfWork, WalletUnitOfWork>();

        // ── Integration Event Mapper ──────────────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, WalletIntegrationEventMapper>();

        // ── Handlers explicites pour les consumers RabbitMQ ───────────────────
        services.AddScoped<CreateWalletCommandHandler>();
        services.AddScoped<DeactivateWalletCommandHandler>();
        services.AddScoped<ExternalProfileActivatedConsumer>();
        services.AddScoped<ExternalProfileSuspendedConsumer>();
        services.AddScoped<MarketplacePaymentRequestedConsumer>();
        services.AddScoped<MarketplacePaymentReleaseRequestedConsumer>();
        services.AddScoped<MarketplacePaymentRefundRequestedConsumer>();

        // ── Core Infrastructure (Wolverine, RabbitMQ, Outbox/Inbox, Logging) ──
        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureWalletDomainQueues();

            var optimizedCs = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 60,
                CommandTimeout = 60,
                MaxPoolSize = 15,
                MinPoolSize = 0,
                Pooling = true,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 10
            }.ToString();

            options.PersistMessagesWithPostgresql(optimizedCs, "wolverine_wallet");

            options.ListenToRabbitQueue(ExchangesConstants.WalletEventsExchange)
                .PreFetchCount(10)
                .UseDurableInbox();

            // Événements du Marketplace Service : traitement des demandes de paiement séquestre
            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.MarketplaceEventsExchange)
                .ToQueue("wallet-service.marketplace-events");

            options.ListenToRabbitQueue("wallet-service.marketplace-events")
                .PreFetchCount(20)
                .MaximumParallelMessages(3)
                .ListenerCount(2)
                .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Discovery.IncludeAssembly(typeof(CreateWalletCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(ExternalProfileActivatedConsumer).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddPostgresDatabase<WalletDbContext>(connectionString, migrationsAssembly);
    }

    public static void BuildInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
        => hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseCoreInfrastructure();

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task PublishWalletResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "WalletService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Registered {Count} permissions for WalletService",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}
