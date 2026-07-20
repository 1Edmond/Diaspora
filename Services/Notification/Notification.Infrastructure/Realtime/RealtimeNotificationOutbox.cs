using Notification.Application.Abstractions;
using Notification.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Realtime;

public sealed class RealtimeNotificationOutbox : IRealtimeNotificationOutbox
{
    private readonly List<PendingRealtimeNotification> _pending = new();

    public void Enqueue(Guid userId, long sequence, Guid notificationId) =>
        _pending.Add(new PendingRealtimeNotification(userId, sequence, notificationId));

    public IReadOnlyList<PendingRealtimeNotification> DequeueAll()
    {
        var items = _pending.ToList();
        _pending.Clear();
        return items;
    }
}
