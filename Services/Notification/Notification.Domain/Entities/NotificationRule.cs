using Notification.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Entities;

public class NotificationRule : AggregateRoot
{
    public string EventType { get; private set; } = string.Empty;
    public NotificationChannel Channels { get; private set; }
    public bool IsActive { get; private set; }

    protected NotificationRule() { }

    public NotificationRule(string eventType, NotificationChannel channels)
    {
        EventType = eventType;
        Channels = channels;
        IsActive = true;
    }
}
