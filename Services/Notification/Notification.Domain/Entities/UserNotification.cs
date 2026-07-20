using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Entities;

public class UserNotification : AggregateRoot
{
    public long Sequence { get; private set; } // ordering SSE / Last-Event-ID — généré par la DB
    public Guid UserId { get; private set; }
    public Guid? MessageId { get; private set; } // corrélation optionnelle avec NotificationLog
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty; // JSON pour le client
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected UserNotification() { }

    public UserNotification(Guid userId, string eventType, string payload, Guid? messageId = null)
    {
        UserId = userId;
        EventType = eventType;
        Payload = payload;
        MessageId = messageId;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public void MarkAsRead(DateTime now)
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = now;
    }
}
