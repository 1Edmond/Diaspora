using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.RabbitMq.Abstractions;
using Shared.Messaging.RabbitMq.Connection;
using Shared.Messaging.RabbitMq.Consumer;
using Shared.Messaging.RabbitMq.Idempotency;
using Shared.Messaging.RabbitMq.Outbox;
using Shared.Messaging.RabbitMq.Publisher;
using Shared.Messaging.RabbitMq.Topology;

namespace Shared.Messaging.RabbitMq;

/// <summary>
/// Extension methods for registering native RabbitMQ messaging services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers native RabbitMQ messaging infrastructure:
    /// connection factory, publisher, outbox dispatcher, idempotency checker, and consumer host.
    /// </summary>
    public static IServiceCollection AddNativeRabbitMq(
        this IServiceCollection services,
        string connectionString,
        string serviceName,
        Action<RabbitMqRegistrationBuilder>? configure = null)
    {
        // Settings
        var settings = new RabbitMqSettings
        {
            ConnectionString = connectionString,
            ServiceName = serviceName
        };
        services.AddSingleton(settings);

        // Connection
        services.AddSingleton<RabbitMqConnectionFactory>();

        // Publisher
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Topology
        services.AddSingleton<RabbitMqTopologyInitializer>();

        // Outbox
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<OutboxDispatcherSettings>();
        services.AddHostedService<OutboxDispatcher>();

        // Idempotency (Inbox)
        services.AddScoped<IIdempotencyChecker, IdempotencyChecker>();

        // Consumer registrations
        var builder = new RabbitMqRegistrationBuilder(services);
        configure?.Invoke(builder);

        // Consumer host
        services.AddHostedService<RabbitMqConsumerHost>();

        return services;
    }
}

/// <summary>
/// Fluent builder for registering message consumers.
/// </summary>
public sealed class RabbitMqRegistrationBuilder
{
    private readonly IServiceCollection _services;

    public RabbitMqRegistrationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Register a consumer for a specific message type on a specific queue.
    /// </summary>
    public RabbitMqRegistrationBuilder AddConsumer<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        _services.AddScoped<TConsumer>();
        _services.AddSingleton(ConsumerRegistration.Create<TMessage, TConsumer>(queueName));
        return this;
    }

    /// <summary>
    /// Configure the outbox dispatcher settings.
    /// </summary>
    public RabbitMqRegistrationBuilder ConfigureOutbox(Action<OutboxDispatcherSettings> configure)
    {
        _services.Configure(configure);
        return this;
    }

    /// <summary>
    /// Register a topology declaration to be applied at startup.
    /// </summary>
    public RabbitMqRegistrationBuilder WithTopology(Action<TopologyDeclaration> configure)
    {
        var topology = new TopologyDeclaration();
        configure(topology);
        _services.AddSingleton(topology);
        return this;
    }
}
