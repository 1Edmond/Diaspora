using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationCommandService
{
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct);
}