using Document.Application;
using Document.Application.Handlers.Commands;
using Document.Application.Interfaces;
using Document.Application.Mappings;
using Document.Infrastructure.BackgroundServices;
using Document.Infrastructure.Extensions;
using Document.Infrastructure.Persistence;
using Document.Infrastructure.Repositories;
using Document.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
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

namespace Document.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        services.AddDocumentApplication();

        var connectionString = configuration.GetConnectionString("DocumentDb")
            ?? throw new InvalidOperationException(
                "Document database connection string is not configured.");

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();

        // ── File Storage — chiffré via Vault + AES-GCM ────────────────────────
        services.AddScoped<IFileStorageService, EncryptedLocalFileStorageService>();

        // ── Integration Event Mapper ──────────────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, DocumentIntegrationEventMapper>();

        // ── Background Service ─────────────────────────────────────────────────
        services.AddHostedService<DocumentExpiryCheckerService>();

        // ── Core Infrastructure (Wolverine, RabbitMQ, Outbox, Vault…) ─────────
        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureDocumentDomainQueues();

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

            options.PersistMessagesWithPostgresql(
                optimizedConnectionString, "wolverine_document");

            options.ListenToRabbitQueue(QueuesConstants.DocumentServiceQueue)
                .PreFetchCount(10)
                .UseDurableInbox();

            options.UseEntityFrameworkCoreTransactions();
            options.Policies.AutoApplyTransactions();

            options.Discovery.IncludeAssembly(
                typeof(UploadDocumentCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        // ── DbContext (inclut les tables DEK de Shared.Security.Persistence) ──
        services.AddPostgresDatabase<DocumentDbContext>(connectionString, migrationsAssembly);
        //services.AddScoped<IApplicationDbContext, DocumentDbContext>();
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
        var context = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
        await context.Database.MigrateAsync();
    }

    public static async Task PublishDocumentResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "DocumentService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Registered {Count} permissions for DocumentService",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}

