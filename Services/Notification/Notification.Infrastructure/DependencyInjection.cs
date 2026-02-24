using JasperFx.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application;
using Notification.Application.Abstractions;
using Notification.Application.Dispatcher;
using Notification.Application.Handlers;
using Notification.Infrastructure.Handlers.Identity;
using Notification.Infrastructure.NotificationSenders;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Processors;
using Notification.Infrastructure.Services;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Domain.Constants;
using Shared.Domain.Models;
using Shared.Infrastructure;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Constants;
using Shared.Security;
using System.Reflection;
using System.Threading.Tasks;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace Notification.Infrastructure;

public static class DependencyInjection
{

    public static async Task AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, Assembly? migrationsAssembly)
    {

        services.AddApplicationDependyInjections();
        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? throw new InvalidOperationException("Notification database connection string is not configured.");


        var elasticSearchSettings = configuration.GetConnectionString("elasticsearch");

        
        services.AddSingleton<NotificationDispatcher>();
        services.AddScoped<INotificationSender, SmsNotificationSender>();
        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IInboxProcessor, InboxProcessor>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHostedService<InboxProcessorBackgroundService>();

       
        services.SetCoreInfrastructureServices(configuration, (options) =>
        {

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

            options.PersistMessagesWithPostgresql(optimizedConnectionString, "wolverine");

            options.Discovery.IncludeAssembly(typeof(IdentityNotificationsEventHandler).Assembly);
            options.Policies.UseDurableInboxOnAllListeners();
            options.Policies.UseDurableOutboxOnAllSendingEndpoints();
            options.ListenToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();
        });
        
        services.AddPostgresDatabase<NotificationDbContext>(connectionString, migrationsAssembly);


    }

    public static void BuildNotificationInfrastructure(this IHostBuilder hostBuilder, IConfiguration configuration,
        string? serviceVersion = null)
    {
        hostBuilder.AddCoreInfrastructure(configuration, serviceVersion);
        hostBuilder.UseResourceSetupOnStartup();
    }

    public static async Task PublishNotificationResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(2));
        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider.GetRequiredService<IResourceDeclarationService>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();

     
        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly("NotificationService", typeof(T).Assembly);
        await messageBus.PublishAsync(declarationEvent);

        logger.LogInformation("🚀 Published {Count} permissions",
            declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}
