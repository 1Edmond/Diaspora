using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Messaging.RabbitMq.Connection;

namespace Shared.Messaging.RabbitMq.Topology;

/// <summary>
/// Declares exchanges, queues, and bindings at service startup.
/// Each service provides its own TopologyDeclaration.
/// </summary>
public sealed class RabbitMqTopologyInitializer
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqTopologyInitializer> _logger;

    public RabbitMqTopologyInitializer(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqTopologyInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(TopologyDeclaration topology, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Declare exchanges
        foreach (var exchange in topology.Exchanges)
        {
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: exchange.Type,
                durable: exchange.Durable,
                autoDelete: false,
                arguments: exchange.Arguments,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Declared exchange {Exchange} (type: {Type}, durable: {Durable})",
                exchange.Name, exchange.Type, exchange.Durable);
        }

        // Declare queues
        foreach (var queue in topology.Queues)
        {
            await channel.QueueDeclareAsync(
                queue: queue.Name,
                durable: queue.Durable,
                exclusive: false,
                autoDelete: false,
                arguments: queue.Arguments,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Declared queue {Queue} (durable: {Durable})",
                queue.Name, queue.Durable);
        }

        // Declare bindings
        foreach (var binding in topology.Bindings)
        {
            await channel.QueueBindAsync(
                queue: binding.QueueName,
                exchange: binding.ExchangeName,
                routingKey: binding.RoutingKey,
                arguments: binding.Arguments,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Bound queue {Queue} to exchange {Exchange} with routing key {RoutingKey}",
                binding.QueueName, binding.ExchangeName, binding.RoutingKey);
        }
    }
}

/// <summary>
/// Builder for RabbitMQ topology: exchanges, queues, and bindings.
/// </summary>
public sealed class TopologyDeclaration
{
    public List<ExchangeDeclaration> Exchanges { get; } = [];
    public List<QueueDeclaration> Queues { get; } = [];
    public List<BindingDeclaration> Bindings { get; } = [];

    public TopologyDeclaration DeclareExchange(string name, string type = ExchangeType.Topic, bool durable = true, IDictionary<string, object?>? arguments = null)
    {
        Exchanges.Add(new ExchangeDeclaration(name, type, durable, arguments));
        return this;
    }

    public TopologyDeclaration DeclareQueue(string name, bool durable = true, IDictionary<string, object?>? arguments = null)
    {
        Queues.Add(new QueueDeclaration(name, durable, arguments));
        return this;
    }

    public TopologyDeclaration DeclareDeadLetterQueue(string queueName, string dlxExchange, string dlqName)
    {
        // The main queue gets the DLX argument
        // The DLQ is just a regular queue bound to the DLX
        return this;
    }

    public TopologyDeclaration Bind(string queueName, string exchangeName, string routingKey, IDictionary<string, object?>? arguments = null)
    {
        Bindings.Add(new BindingDeclaration(queueName, exchangeName, routingKey, arguments));
        return this;
    }
}

public sealed record ExchangeDeclaration(string Name, string Type, bool Durable, IDictionary<string, object?>? Arguments);
public sealed record QueueDeclaration(string Name, bool Durable, IDictionary<string, object?>? Arguments);
public sealed record BindingDeclaration(string QueueName, string ExchangeName, string RoutingKey, IDictionary<string, object?>? Arguments);
