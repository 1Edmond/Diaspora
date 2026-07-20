using Notification.Application.DTOs;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationQueryService
{
    Task<PagedList<UserNotificationDto>> GetUserNotificationsAsync(
       Guid userId, int pageNumber, int pageSize, bool unreadOnly, CancellationToken ct);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct);
}