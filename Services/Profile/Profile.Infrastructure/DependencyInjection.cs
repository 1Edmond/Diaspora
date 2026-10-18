using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Profile.Application;
using Profile.Application.Handlers.Commands;
using Profile.Application.Handlers.IntegrationEvents;
using Profile.Application.Interfaces;
using Profile.Application.Mappings;
using Profile.Infrastructure.Consumers;
using Profile.Infrastructure.Persistence;
using Profile.Infrastructure.Repositories;
using Shared.Application.Abstractions;
using Shared.Domain.Interfaces;
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

namespace Profile.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        services.AddProfileApplication();

        var connectionString = configuration.GetConnectionString("ProfileDb")
            ?? throw new InvalidOperationException(
                "Profile database connection string is not configured.");

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IProfileTypeRepository, ProfileTypeRepository>();
        services.AddScoped<IMobileConfigRepository, MobileConfigRepository>();

        // ── Integration Event Mapper (Outbox) ─────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, ProfileIntegrationEventMapper>();

        // ── Handler enregistré explicitement pour le consumer RabbitMQ ────────
        services.AddScoped<ActivateProfileCommandHandler>();
        services.AddScoped<KycArrivalProofApprovedConsumer>();
       
        // ── Core infrastructure (Wolverine, RabbitMQ, Outbox, Logging…) ───────

        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureProfileDomainQueues();

            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.IdentityEventsExchange)
                   .ToQueue("profile-service.identity-events");

            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.KycEventsExchange)
                   .ToQueue("profile-service.kyc-events");

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
            
            // Persistance des messages Wolverine dans PostgreSQL
            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine");

            // Queue entrante : événements Identity
            options.ListenToRabbitQueue("profile-service.identity-events")
              .PreFetchCount(20)
              .MaximumParallelMessages(3)
              .ListenerCount(2)
              .UseDurableInbox();

            // Queue entrante : événements KYC
            options.ListenToRabbitQueue("profile-service.kyc-events")
              .PreFetchCount(20)
              .MaximumParallelMessages(3)
              .ListenerCount(2)
              .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Discovery.IncludeAssembly(
                typeof(ActivateProfileCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(
                typeof(KycArrivalProofApprovedConsumer).Assembly);

            options.Discovery.IncludeAssembly(typeof(UserActivatedIntegrationEventHandler).Assembly);

            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });
        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddPostgresDatabase<ProfileDbContext>(connectionString, migrationsAssembly); 

        //services.AddScoped<IApplicationDbContext, ProfileDbContext>();
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
        var context = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

        await context.Database.EnsureCreatedAsync();
    }

    public static async Task PublishProfileResources<T>(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "ProfileService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Published {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}