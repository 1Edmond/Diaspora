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
using Notification.Application.Services;
using Notification.Infrastructure.Handlers.Identity;
using Notification.Infrastructure.Mappers;
using Notification.Infrastructure.NotificationSenders;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Processors;
using Notification.Infrastructure.Realtime;
using Notification.Infrastructure.Services;
using Npgsql;
using Shared.Application.Abstractions;
using Shared.Domain.Constants;
using Shared.Domain.Models;
using Shared.Infrastructure;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Constants;
using Shared.Security;
using StackExchange.Redis;
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

        services.AddScoped<INotificationApplicationDbContext, NotificationDbContext>();
        services.AddSingleton<IConnectionManager, ConnectionManager>();
        services.AddSingleton<INotificationRealtimePublisher, RedisNotificationPublisher>();
        services.AddHostedService<RedisNotificationSubscriberService>();
        services.AddScoped<IInAppNotificationPayloadMapper, ProfileCreatedPayloadMapper>();
        services.AddScoped<IRealtimeNotificationOutbox, RealtimeNotificationOutbox>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<INotificationCommandService, NotificationCommandService>();
        services.AddScoped<INotificationHistoryService>(sp =>
            (INotificationHistoryService)sp.GetRequiredService<INotificationQueryService>());
        services.AddScoped<INotificationRuleService, NotificationRuleService>();
        services.AddSingleton<NotificationDispatcher>();
        services.AddScoped<INotificationSender, SmsNotificationSender>();
        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationSender, InAppNotificationSender>();
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

            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.IdentityEventsExchange)
                   .ToQueue("notification-service.identity-events")
                   .BindExchange(ExchangesConstants.ProfileEventsExchange)
                   .ToQueue("notification-service.profile-events")
                   .BindExchange(ExchangesConstants.MarketplaceEventsExchange)
                   .ToQueue("notification-service.marketplace-events")
                   .BindExchange(ExchangesConstants.FreelanceEventsExchange)
                   .ToQueue("notification-service.freelance-events")
                   .BindExchange(ExchangesConstants.KycEventsExchange)
                   .ToQueue("notification-service.kyc-events")
                   .BindExchange(ExchangesConstants.WalletEventsExchange)
                   .ToQueue("notification-service.wallet-events");


            options.Discovery.IncludeAssembly(typeof(IdentityNotificationsEventHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(ProfileNotificationEventHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(MarketplaceNotificationEventHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(FreelanceNotificationEventHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(KYCNotificationsEventHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(WalletNotificationEventHandler).Assembly);
            options.Policies.UseDurableInboxOnAllListeners();
            options.Policies.UseDurableOutboxOnAllSendingEndpoints();
            options.ListenToRabbitQueue("notification-service.identity-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();
            
            options.ListenToRabbitQueue("notification-service.profile-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();

            options.ListenToRabbitQueue("notification-service.marketplace-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();

            options.ListenToRabbitQueue("notification-service.freelance-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();

            options.ListenToRabbitQueue("notification-service.kyc-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();

            options.ListenToRabbitQueue("notification-service.wallet-events")
              .PreFetchCount(15)
              .MaximumParallelMessages(5)
              .ListenerCount(2)
              .UseDurableInbox();
        });
        
        services.AddPostgresDatabase<NotificationDbContext>(connectionString, migrationsAssembly);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var cs = config.GetConnectionString("redis")
                ?? throw new InvalidOperationException("Redis connection string 'redis' not found in configuration.");
            return ConnectionMultiplexer.Connect(cs);
        });

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
