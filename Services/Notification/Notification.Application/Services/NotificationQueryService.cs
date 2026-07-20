using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Models;
using Notification.Domain.Entities;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Services;

public sealed class NotificationQueryService : INotificationQueryService, INotificationHistoryService
{
    private readonly INotificationApplicationDbContext _db;
    public NotificationQueryService(INotificationApplicationDbContext db) => _db = db;

    private static UserNotificationDto ToDto(UserNotification n) =>
        new(n.Id, n.Sequence, n.EventType, n.Payload, n.IsRead, n.CreatedAt, n.ReadAt);

    public async Task<PagedList<UserNotificationDto>> GetUserNotificationsAsync(
        Guid userId, int pageNumber, int pageSize, bool unreadOnly, CancellationToken ct)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.UserNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var projected = query
            .OrderByDescending(n => n.Sequence)
            .Select(n => ToDto(n));

        // ⚠️ PagedList<T>.CreateAsync utilise Count()/.ToList() SYNCHRONES en interne
        // (voir remarque précédente — à corriger si tu veux du vrai async)
        return await PagedList<UserNotificationDto>.CreateAsync(projected, pageNumber, pageSize, ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct) =>
        await _db.UserNotifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<IReadOnlyList<UserNotificationDto>> GetNotificationsAfterAsync(
        Guid userId, long lastSequence, CancellationToken ct) =>
        await _db.UserNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Sequence > lastSequence)
            .OrderBy(n => n.Sequence)
            .Select(n => ToDto(n))
            .ToListAsync(ct);
}
