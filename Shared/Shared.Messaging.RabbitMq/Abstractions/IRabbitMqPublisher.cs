namespace Shared.Messaging.RabbitMq.Abstractions;

/// <summary>
/// Publishes messages to RabbitMQ with the required headers and properties.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publish a message directly to RabbitMQ (bypass outbox — used by OutboxDispatcher).
    /// </summary>
    Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        Guid messageId,
        string? correlationId = null,
        string? traceId = null,
        string? partnerId = null,
        string? schemaVersion = null,
        CancellationToken cancellationToken = default) where TMessage : class;
}
