using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationCommandService
{
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct);
    Task<Guid> BroadcastNotificationAsync(string title, string message, string channel, string? targetRole, Guid? profileTypeId, CancellationToken ct);
}