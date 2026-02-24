using Shared.Domain.Primitives;

namespace Shared.Infrastructure.Inbox;

public class InboxMessage : AggregateRoot
{
    // Identification
    public Guid MessageId { get; private set; }
    public string MessageType { get; private set; } = string.Empty;

    // Contenu
    public string Content { get; private set; } = string.Empty; // JSON serialized

    // Statut
    public InboxMessageStatus Status { get; private set; }

    // Timestamps
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Gestion des erreurs
    public int ProcessingAttempts { get; private set; }
    public string? Error { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }

    // Traçabilité
    public string? CorrelationId { get; private set; }
    public string? SourceSystem { get; private set; }

    // Constructor pour EF Core
    protected InboxMessage() { }

    public InboxMessage(
        Guid messageId,
        string messageType,
        string content,
        string? correlationId = null,
        string? sourceSystem = null)
    {
        MessageId = messageId;
        MessageType = messageType;
        Content = content;
        Status = InboxMessageStatus.Pending;
        ReceivedAt = DateTime.UtcNow;
        ProcessingAttempts = 0;
        CorrelationId = correlationId;
        SourceSystem = sourceSystem;
    }

    public void MarkAsProcessing()
    {
        if (Status == InboxMessageStatus.Processed)
            throw new InvalidOperationException("Message already processed");

        Status = InboxMessageStatus.Processing;
        ProcessingAttempts++;
        LastAttemptAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        Status = InboxMessageStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        Status = InboxMessageStatus.Failed;
        Error = error;
        LastAttemptAt = DateTime.UtcNow;
    }

    public bool CanRetry(int maxAttempts = 3)
    {
        return Status == InboxMessageStatus.Failed
            && ProcessingAttempts < maxAttempts;
    }
}

