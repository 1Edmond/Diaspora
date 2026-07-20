using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationApplicationDbContext
{
    DbSet<NotificationRule> NotificationRules { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
    DbSet<UserNotification> UserNotifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
