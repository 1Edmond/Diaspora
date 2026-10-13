using Notification.Domain.Entities;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Abstractions;

public interface INotificationRuleService
{
    Task<PagedList<NotificationRuleDto>> GetRulesAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<NotificationRuleDto?> GetRuleAsync(string eventType, CancellationToken ct);
    Task<NotificationRuleDto> UpsertRuleAsync(NotificationRuleDto dto, CancellationToken ct);
    Task<bool> DeleteRuleAsync(string eventType, CancellationToken ct);
}

public record NotificationRuleDto(
    string EventType,
    int Channels,
    bool IsActive,
    string[] ChannelNames
);