using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Infrastructure.Inbox;

public enum InboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
