using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Messaging.RabbitMq.Abstractions;
using Shared.Messaging.RabbitMq.Connection;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.RabbitMq.Publisher;

/// <summary>
/// Publishes messages to RabbitMQ with mandatory headers:
/// MessageId, CorrelationId, Timestamp, ContentType, x-trace-id, x-partner-id, x-schema-version.
/// </summary>
public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        Guid messageId,
        string? correlationId = null,
        string? traceId = null,
        string? partnerId = null,
        string? schemaVersion = null,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var properties = new BasicProperties
        {
            MessageId = messageId.ToString(),
            CorrelationId = correlationId ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>()
        };

        if (!string.IsNullOrEmpty(traceId))
            properties.Headers["x-trace-id"] = Encoding.UTF8.GetBytes(traceId);

        if (!string.IsNullOrEmpty(partnerId))
            properties.Headers["x-partner-id"] = Encoding.UTF8.GetBytes(partnerId);

        if (!string.IsNullOrEmpty(schemaVersion))
            properties.Headers["x-schema-version"] = Encoding.UTF8.GetBytes(schemaVersion);

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published message {MessageId} to {Exchange}/{RoutingKey} (type: {MessageType})",
            messageId, exchange, routingKey, typeof(TMessage).Name);
    }
}
