using JasperFx.Resources;
using KYC.Application;
using KYC.Application.Abstractions;
using KYC.Application.Handlers.IntegrationEvents;
using KYC.Infrastructure.Persistence;
using KYC.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Constants;
using Shared.Domain.Models;
using Shared.Infrastructure;
using Shared.Messaging.Constants;
using Shared.Security;
using System.Reflection;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace KYC.Infrastructure;

public static class DependencyInjection
{

    public static async Task AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Assembly? migrationsAssembly)
    {

        services.AddApplicationDependyInjections();
        var connectionString = configuration.GetConnectionString("KycDb")
            ?? throw new InvalidOperationException("KYC database connection string is not configured.");
        
        
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.SetCoreInfrastructureServices(configuration, (options) =>
        {
            //options.PersistMessagesWithPostgresql(connectionString);
            //options.Discovery.IncludeAssembly(typeof(UserDeletedIntegrationEventHandler).Assembly);
            //options.Policies.UseDurableInboxOnAllListeners();
            //options.Policies.UseDurableOutboxOnAllSendingEndpoints();
            //options.Policies.AutoApplyTransactions();
            //options.ListenToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue);
        });

        services.AddPostgresDatabase<KYCDbContext>(connectionString, migrationsAssembly);
        services.AddScoped<IKYCDbContext, KYCDbContext>();

    }

    public static void BuildKYCInfrastructure(this IHostBuilder hostBuilder, IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);
        hostBuilder.UseResourceSetupOnStartup();
    }

    public static async void UseInfrastructure(this IApplicationBuilder app)
    {
        
        app.UseCoreInfrastructure();

         using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KYCDbContext>();

        await db.Database.MigrateAsync();
    }

    public static async Task PublishKYCResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(2));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("KYCService", typeof(T).Assembly);
        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("🚀 Published {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }


}
