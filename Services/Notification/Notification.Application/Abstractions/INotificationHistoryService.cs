using Notification.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationHistoryService
{
    Task<IReadOnlyList<UserNotificationDto>> GetNotificationsAfterAsync(
            Guid userId, long lastSequence, CancellationToken ct);
}