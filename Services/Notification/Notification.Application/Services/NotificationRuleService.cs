using Microsoft.EntityFrameworkCore;
using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Services;

public sealed class NotificationRuleService : INotificationRuleService
{
    private readonly INotificationApplicationDbContext _db;

    public NotificationRuleService(INotificationApplicationDbContext db) => _db = db;

    public async Task<PagedList<NotificationRuleDto>> GetRulesAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.NotificationRules
            .AsNoTracking()
            .OrderBy(r => r.EventType);

        return await PagedList<NotificationRuleDto>.CreateAsync(
            query.Select(r => ToDto(r)),
            pageNumber, pageSize, ct);
    }

    public async Task<NotificationRuleDto?> GetRuleAsync(string eventType, CancellationToken ct)
    {
        var rule = await _db.NotificationRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventType == eventType, ct);

        return rule != null ? ToDto(rule) : null;
    }

    public async Task<NotificationRuleDto> UpsertRuleAsync(NotificationRuleDto dto, CancellationToken ct)
    {
        var rule = await _db.NotificationRules
            .FirstOrDefaultAsync(r => r.EventType == dto.EventType, ct);

        var channels = (NotificationChannel)dto.Channels;

        if (rule == null)
        {
            rule = new NotificationRule(dto.EventType, channels);
            rule.Update(channels, dto.IsActive);
            _db.NotificationRules.Add(rule);
        }
        else
        {
            rule.Update(channels, dto.IsActive);
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(rule);
    }

    public async Task<bool> DeleteRuleAsync(string eventType, CancellationToken ct)
    {
        var rule = await _db.NotificationRules
            .FirstOrDefaultAsync(r => r.EventType == eventType, ct);

        if (rule == null) return false;

        _db.NotificationRules.Remove(rule);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static NotificationRuleDto ToDto(NotificationRule r) =>
        new(
            r.EventType,
            (int)r.Channels,
            r.IsActive,
            Enum.GetValues<NotificationChannel>()
                .Where(c => r.Channels.HasFlag(c))
                .Select(c => c.ToString())
                .ToArray());
}