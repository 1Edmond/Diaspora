using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Entities;

public class NotificationLog : AggregateRoot
{
    public Guid MessageId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }

    protected NotificationLog() { }

    public NotificationLog(Guid messageId, string eventType)
    {
        MessageId = messageId;
        EventType = eventType;
        SentAt = DateTime.UtcNow;
    }
}