using Chat.Application;
using Chat.Application.Handlers.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Mappings;
using Chat.Infrastructure.Consumers;
using Chat.Infrastructure.Extensions;
using Chat.Infrastructure.Persistence;
using Chat.Infrastructure.Repositories;
using Chat.Infrastructure.Services;
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
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace Chat.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? migrationsAssembly)
    {
        services.AddChatApplication();

        var connectionString = configuration.GetConnectionString("ChatDb")
            ?? throw new InvalidOperationException(
                "Chat database connection string is not configured.");

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IChatProfileRepository, ChatProfileRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ICallRepository, CallRepository>();

        // ── UnitOfWork ────────────────────────────────────────────────────────
       // services.AddScoped<IUnitOfWork, ChatUnitOfWork>();

        // ── Integration Event Mapper ──────────────────────────────────────────
        services.AddScoped<IIntegrationEventMapper, ChatIntegrationEventMapper>();

        // ── Notification Service (SignalR push) ───────────────────────────────
        services.AddScoped<IChatNotificationService, ChatNotificationService>();

        // ── Handlers explicites pour les consumers ────────────────────────────
        services.AddScoped<CreateChatProfileCommandHandler>();
        services.AddScoped<ExternalProfileActivatedConsumer>();
        services.AddScoped<ExternalProfileSuspendedConsumer>();

        // ── SignalR ───────────────────────────────────────────────────────────
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = false;
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });

        // ── Core Infrastructure (Wolverine, RabbitMQ, Outbox/Inbox) ──────────
        services.SetCoreInfrastructureServices(configuration, options =>
        {
            options.ConfigureChatDomainQueues();

            var optimizedCs = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 60,
                CommandTimeout = 60,
                MaxPoolSize = 20,
                MinPoolSize = 0,
                Pooling = true,
                ConnectionIdleLifetime = 300,
                ConnectionPruningInterval = 10
            }.ToString();

            options.PersistMessagesWithPostgresql(optimizedCs, "wolverine_chat");

            options.ConfigureRabbitMq().BindExchange(ExchangesConstants.ProfileEventsExchange)
                  .ToQueue("chat-service.profile-events");

            // Queue entrante : événements Profiles
            options.ListenToRabbitQueue("chat-service.profile-events")
              .PreFetchCount(20)
              .MaximumParallelMessages(3)
              .ListenerCount(2)
              .UseDurableInbox();

            // Handlers gèrent leur propre persistance via IUnitOfWork.
            options.Discovery.IncludeAssembly(typeof(CreateChatProfileCommandHandler).Assembly);
            options.Discovery.IncludeAssembly(typeof(ExternalProfileActivatedConsumer).Assembly);
            options.Discovery.IncludeAssembly(migrationsAssembly!);
        });

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddPostgresDatabase<ChatDbContext>(connectionString, migrationsAssembly);
       // services.AddScoped<IApplicationDbContext, ChatDbContext>();
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
        var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public static async Task PublishChatResources<T>(this WebApplication app)
    {
        await Task.Delay(TimeSpan.FromMinutes(1));

        using var scope = app.Services.CreateScope();
        var resourceService = scope.ServiceProvider
            .GetRequiredService<IResourceDeclarationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<T>>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var declarationEvent = resourceService.BuildDeclarationEventFromAssembly(
            "ChatService", typeof(T).Assembly);

        await messageBus.PublishAsync(declarationEvent);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Registered {Count} permissions for ChatService",
                declarationEvent.Resources.Sum(r => r.Permissions.Count));
    }
}
