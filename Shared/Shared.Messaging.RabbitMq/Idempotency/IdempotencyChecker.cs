using Microsoft.EntityFrameworkCore;

namespace Shared.Messaging.RabbitMq.Idempotency;

/// <summary>
/// Interface for checking and marking message idempotency.
/// </summary>
public interface IIdempotencyChecker
{
    /// <summary>
    /// Returns true if the message has already been processed by this consumer.
    /// </summary>
    Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed by this consumer.
    /// Should be called within the same transaction as the business logic.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);
}

/// <summary>
/// EF Core implementation of the idempotency checker.
/// </summary>
public sealed class IdempotencyChecker : IIdempotencyChecker
{
    private readonly DbContext _dbContext;

    public IdempotencyChecker(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasBeenProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProcessedMessage>()
            .AnyAsync(m => m.MessageId == messageId && m.ConsumerName == consumerName,
                cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<ProcessedMessage>().AddAsync(new ProcessedMessage
        {
            MessageId = messageId,
            ConsumerName = consumerName,
            ProcessedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
