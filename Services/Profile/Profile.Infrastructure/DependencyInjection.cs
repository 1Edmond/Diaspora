using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Profile.Application;
using Profile.Application.Handlers.Commands;
using Profile.Application.Interfaces;
using Profile.Application.Mappings;
using Profile.Infrastructure.Consumers;
using Profile.Infrastructure.Extensions;
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
        services.AddScoped<IInternalProfileRepository, InternalProfileRepository>();
        services.AddScoped<IExternalProfileRepository, ExternalProfileRepository>();

        // ── Integration Event Mapper (Outbox) ─────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, ProfileIntegrationEventMapper>();

        // ── Handler enregistré explicitement pour le consumer RabbitMQ ────────
        services.AddScoped<ActivateExternalProfileCommandHandler>();
        services.AddScoped<KycArrivalProofApprovedConsumer>();

        // ── Core infrastructure (Wolverine, RabbitMQ, Outbox, Logging…) ───────
        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureProfileDomainQueues();

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
            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine_profile");

            // Queue entrante : événements KYC → Profile
            options.ListenToRabbitQueue(QueuesConstants.ProfileServiceQueue)
                .PreFetchCount(10)
                .UseDurableInbox();

            // Transactions EF Core automatiques
            options.UseEntityFrameworkCoreTransactions();
            options.Policies.AutoApplyTransactions();

            // Scan des handlers Application + Infrastructure + migrations
            options.Discovery.IncludeAssembly(
                typeof(ActivateExternalProfileCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(
                typeof(KycArrivalProofApprovedConsumer).Assembly);
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

        await context.Database.MigrateAsync();
    }

    public static async Task PublishProfileResources<T>(this WebApplication app)
    {
        // Délai pour laisser le temps aux autres services de démarrer
        await Task.Delay(TimeSpan.FromMinutes(1));

        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "ProfileService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("🚀 Published {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}