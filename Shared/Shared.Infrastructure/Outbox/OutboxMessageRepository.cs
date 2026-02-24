using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Outbox;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly DbContext _context;

    public OutboxMessageRepository(DbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.Set<OutboxMessage>().AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>().FindAsync(new object[] { id }, cancellationToken);
        if (message != null)
        {
            message.ProcessedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid id, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>().FindAsync(new object[] { id }, cancellationToken);
        if (message != null)
        {
            message.Error = error;
            message.RetryCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

