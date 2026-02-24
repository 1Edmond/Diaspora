using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Inbox;

public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<InboxMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task SaveInboxMessageAsync(InboxMessage message, CancellationToken cancellationToken = default);
    Task<T?> ProcessOnceAsync<T>(
        Guid messageId,
        string messageType,
        Func<Task<T>> action,
        CancellationToken cancellationToken = default);
}
