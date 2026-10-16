
using IdentityService.Application;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands;
using IdentityService.Application.Handlers.Commands;
using IdentityService.Application.Handlers.IntegrationEvents;
using IdentityService.Application.Handlers.Queries;
using IdentityService.Handlers;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Seed;
using IdentityService.Infrastructure.Services;
using JasperFx.Core.TypeScanning;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Domain.Constants;
using Shared.Domain.Models;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;
using Shared.Security;
using Shared.Security.Abstractions;
using System.Reflection;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Transports;


namespace IdentityService.Infrastructure;

public static class DependencyInjection
{

    public static async Task AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Assembly? migrationsAssembly)
    {
        services.AddApplicationDependyInjections();

        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Identity database connection string is not configured.");

       
        
       
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<IResourcePermissionRepository, ResourcePermissionRepository>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<ResourcesDeclarationEventHandler>();
        services.SetCoreInfrastructureServices(configuration, (options) =>
        {
            options.ConfigureIdentityDomainQueues();

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

            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine_identity");
            
            options.ListenToRabbitQueue(ExchangesConstants.IdentityEventsExchange)
              .PreFetchCount(20)
              .MaximumParallelMessages(5)
              .ListenerCount(6)
              .UseDurableInbox();
            // Note : AutoApplyTransactions et UseEntityFrameworkCoreTransactions créent une ambiguïté Wolverine
            // car AddPostgresDatabase enregistre à la fois IdentityDbContext et DbContext. Les handlers gèrent leur propre persistance.
            options.Discovery.IncludeAssembly(typeof(RegisterUserCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(KYCVerificationApprovedIntegrationEventHandler).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });
 
        services.AddPostgresDatabase<IdentityDbContext>(connectionString, migrationsAssembly);
        services.AddScoped<IApplicationDbContext, IdentityDbContext>();
        services.AddScoped<IEd25519SignatureVerifier, Ed25519SignatureVerifier>();
        services.AddScoped<IBiometricChallengeStore, RedisBiometricChallengeStore>();
        
    }

    public static void BuildInfrastructure(this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(
            configuration,
            serviceVersion);
    }

    public static async Task UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseCoreInfrastructure();

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await context.Database.MigrateAsync();

        var roleSeeder = new RoleSeeder(
            context,
            scope.ServiceProvider.GetRequiredService<ILogger<RoleSeeder>>()
        );
        await roleSeeder.SeedAsync();

        var adminSeeder = new AdminUserSeeder(
            context,
            scope.ServiceProvider.GetRequiredService<IHashingService>(),
            scope.ServiceProvider.GetRequiredService<ILogger<AdminUserSeeder>>()
        );
        await adminSeeder.SeedAsync();
    }

    public static async Task PublishIdentityResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();


        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("IdentityService", typeof(T).Assembly);
        var handler = scope.ServiceProvider.GetRequiredService<ResourcesDeclarationEventHandler>();
        await handler.Handle(declarationEvent);

        if(logger.IsEnabled(LogLevel.Information))
        logger.LogInformation("🚀 Registered {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }


}
