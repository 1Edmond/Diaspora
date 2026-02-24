namespace Shared.Messaging.RabbitMq.Abstractions;

/// <summary>
/// Consumer interface for processing RabbitMQ messages.
/// Each implementation handles a specific message type with explicit Ack/Nack.
/// </summary>
public interface IMessageConsumer<in TMessage> where TMessage : class
{
    /// <summary>
    /// Process the incoming message. 
    /// If this method completes successfully, the message will be BasicAck'd.
    /// If it throws, the message will be BasicNack'd (requeue=false → dead-letter).
    /// </summary>
    Task ConsumeAsync(TMessage message, MessageContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context information passed alongside each consumed message.
/// </summary>
public sealed record MessageContext(
    Guid MessageId,
    string CorrelationId,
    string? TraceId,
    string? PartnerId,
    string? SchemaVersion,
    DateTimeOffset Timestamp);
