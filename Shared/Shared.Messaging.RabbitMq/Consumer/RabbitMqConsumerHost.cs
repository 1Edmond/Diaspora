using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.RabbitMq.Abstractions;
using Shared.Messaging.RabbitMq.Connection;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq.Consumer;

/// <summary>
/// Background service that listens on RabbitMQ queues and routes messages
/// to the appropriate IMessageConsumer&lt;T&gt; implementation.
/// Handles BasicAck on success, BasicNack (requeue=false → DLQ) on failure.
/// </summary>
public sealed class RabbitMqConsumerHost : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqConsumerHost> _logger;
    private readonly List<ConsumerRegistration> _registrations;
    private IChannel? _channel;

    public RabbitMqConsumerHost(
        RabbitMqConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqConsumerHost> logger,
        IEnumerable<ConsumerRegistration> registrations)
    {
        _connectionFactory = connectionFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _registrations = registrations.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionFactory.GetConnectionAsync(stoppingToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Set prefetch count for fair dispatch
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        foreach (var registration in _registrations)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleMessageAsync(registration, ea, stoppingToken);
            };

            await _channel.BasicConsumeAsync(
                queue: registration.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Started consuming queue {Queue} with consumer {ConsumerType}",
                registration.QueueName, registration.MessageType.Name);
        }

        // Keep the service running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(
        ConsumerRegistration registration,
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
    {
        var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();

        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

            var message = JsonSerializer.Deserialize(body, registration.MessageType, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (message is null)
            {
                _logger.LogError(
                    "Failed to deserialize message {MessageId} from queue {Queue}",
                    messageId, registration.QueueName);

                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                return;
            }

            var context = ExtractMessageContext(ea);

            // Resolve the consumer from DI and invoke
            using var scope = _serviceProvider.CreateScope();
            await registration.InvokeConsumerAsync(scope.ServiceProvider, message, context, cancellationToken);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Successfully processed message {MessageId} from queue {Queue}",
                messageId, registration.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process message {MessageId} from queue {Queue}",
                messageId, registration.QueueName);

            // Nack without requeue → message goes to DLQ
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
        }
    }

    private static MessageContext ExtractMessageContext(BasicDeliverEventArgs ea)
    {
        var props = ea.BasicProperties;
        var headers = props?.Headers;

        return new MessageContext(
            MessageId: Guid.TryParse(props?.MessageId, out var mid) ? mid : Guid.NewGuid(),
            CorrelationId: props?.CorrelationId ?? string.Empty,
            TraceId: GetHeaderString(headers, "x-trace-id"),
            PartnerId: GetHeaderString(headers, "x-partner-id"),
            SchemaVersion: GetHeaderString(headers, "x-schema-version"),
            Timestamp: props?.Timestamp is { UnixTime: > 0 }
                ? DateTimeOffset.FromUnixTimeSeconds(props.Timestamp.UnixTime)
                : DateTimeOffset.UtcNow);
    }

    private static string? GetHeaderString(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string str => str,
            _ => value?.ToString()
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
