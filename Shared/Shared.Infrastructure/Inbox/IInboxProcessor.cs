using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Inbox;

public interface IInboxProcessor
{
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
    Task ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
}
