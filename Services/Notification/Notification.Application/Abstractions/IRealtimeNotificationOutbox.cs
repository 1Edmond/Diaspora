using Notification.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface IRealtimeNotificationOutbox
{
    void Enqueue(Guid userId, long sequence, Guid notificationId);
    IReadOnlyList<PendingRealtimeNotification> DequeueAll();
}