using Microsoft.EntityFrameworkCore;

namespace Shared.Messaging.RabbitMq.Outbox;

/// <summary>
/// Repository for outbox message CRUD operations.
/// </summary>
public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsSentAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

/// <summary>
/// EF Core implementation of the outbox repository.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly DbContext _dbContext;

    public OutboxRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<OutboxMessage>().AddAsync(message, cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsSentAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<OutboxMessage>()
            .Where(m => m.MessageId == messageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, OutboxMessageStatus.Sent)
                .SetProperty(m => m.SentAt, DateTime.UtcNow),
                cancellationToken: cancellationToken);
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<OutboxMessage>()
            .Where(m => m.MessageId == messageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, OutboxMessageStatus.Failed)
                .SetProperty(m => m.Error, error)
                .SetProperty(m => m.RetryCount, m => m.RetryCount + 1),
                cancellationToken: cancellationToken);
    }
}
