using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Services;

public sealed class NotificationCommandService : INotificationCommandService
{
    private readonly INotificationApplicationDbContext _db;
    public NotificationCommandService(INotificationApplicationDbContext db) => _db = db;

    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct)
    {
        var n = await _db.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);

        if (n is null) return false;

        n.MarkAsRead(DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _db.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), ct);
    }
}