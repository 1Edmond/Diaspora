using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Procedure.Application;
using Procedure.Application.Handlers.Commands;
using Procedure.Application.Handlers.Queries;
using Procedure.Application.DTOs;
using Procedure.Application.Interfaces;
using Procedure.Application.Mappings;
using Procedure.Infrastructure.Extensions;
using Procedure.Infrastructure.Persistences;
using Procedure.Infrastructure.Repositories;
using Shared.Application.Abstractions;
using Shared.Application.Handlers.Commands;
using Shared.Application.Handlers.Queries;
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

namespace Procedure.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        services.AddProcedureApplication();

        var connectionString = configuration.GetConnectionString("ProcedureDb")
            ?? throw new InvalidOperationException(
                "Procedure database connection string is not configured.");

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IProcedureRepository, ProcedureRepository>();
        services.AddScoped<IUserProcedureRepository, UserProcedureRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IServiceParameterRepository, ServiceParameterRepository>();

        // ── Integration Event Mapper ──────────────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, ProcedureIntegrationEventMapper>();

        // ── Translation handlers ───────────────────────────────────────────────
        services.AddScoped(typeof(CreateWithTranslationsCommandHandler<Procedure.Domain.Entities.Procedure, ProcedureDto>),
            typeof(CreateProcedureWithTranslationsCommandHandler));
        services.AddScoped(typeof(UpdateWithTranslationsCommandHandler<Procedure.Domain.Entities.Procedure, ProcedureDto>),
            typeof(UpdateProcedureWithTranslationsCommandHandler));
        services.AddScoped(typeof(SetTranslationCommandHandler<Procedure.Domain.Entities.Procedure>),
            typeof(SetProcedureTranslationCommandHandler));
        services.AddScoped(typeof(DeleteTranslationCommandHandler<Procedure.Domain.Entities.Procedure>),
            typeof(DeleteProcedureTranslationCommandHandler));
        services.AddScoped(typeof(GetTranslationsQueryHandler<Procedure.Domain.Entities.Procedure>),
            typeof(GetProcedureTranslationsQueryHandler));

        // ── Core Infrastructure ───────────────────────────────────────────────
        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureProcedureDomainQueues();

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
                optimizedConnectionString, "wolverine_procedure");

            options.ListenToRabbitQueue(ExchangesConstants.ProcedureEventsExchange)
                .PreFetchCount(10)
                .UseDurableInbox();


            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.ProfileEventsExchange)
                .ToQueue("procedure-service.profile-events");

            // Queue entrante : événements Profiles
            options.ListenToRabbitQueue("procedure-service.profile-events")
              .PreFetchCount(20)
              .MaximumParallelMessages(3)
              .ListenerCount(2)
              .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Discovery.IncludeAssembly(
                typeof(CreateProcedureCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddPostgresDatabase<ProcedureDbContext>(connectionString, migrationsAssembly);
       // services.AddScoped<IApplicationDbContext, ProcedureDbContext>();
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
        var context = scope.ServiceProvider.GetRequiredService<ProcedureDbContext>();


        await context.Database.MigrateAsync();

        var seeder = new ProcedureSeeder(context, scope.ServiceProvider.GetRequiredService<ILogger<ProcedureSeeder>>());
        await seeder.SeedAsync();
    }

    public static async Task PublishProcedureResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "ProcedureService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Publish {Count} permissions for ProcedureService",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}
