using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationRealtimePublisher : INotificationSender
{
    Task PublishToUserAsync(Guid userId, long sequence, Guid notificationId, CancellationToken ct = default);
}