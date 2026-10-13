using Microsoft.AspNetCore.Mvc;
using Notification.Application.Abstractions;
using Shared.Security.Attributes;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/notification-rules")]
public sealed class NotificationRulesController(
    INotificationRuleService _ruleService) : ControllerBase
{
    [Permission(
        resourceKey: "notification-rule",
        action: "read",
        description: "List notification rules",
        recommendedRoles: new[] { "Admin", "User" }
    )]
    [HttpGet]
    public async Task<ActionResult<Shared.Application.Models.PagedList<NotificationRuleDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) =>
        Ok(await _ruleService.GetRulesAsync(pageNumber, pageSize, ct));

    [Permission(
        resourceKey: "notification-rule",
        action: "read",
        description: "Get notification rule by event type",
        recommendedRoles: new[] { "Admin", "User" }
    )]
    [HttpGet("{eventType}")]
    public async Task<ActionResult<NotificationRuleDto>> GetByEventType(
        string eventType, CancellationToken ct)
    {
        var rule = await _ruleService.GetRuleAsync(eventType, ct);
        return rule != null ? Ok(rule) : NotFound();
    }

    [Permission(
        resourceKey: "notification-rule",
        action: "create",
        description: "Create or update notification rule",
        recommendedRoles: new[] { "Admin", "User" }
    )]
    [HttpPut("{eventType}")]
    public async Task<ActionResult<NotificationRuleDto>> Upsert(
        string eventType,
        [FromBody] UpsertNotificationRuleRequest request,
        CancellationToken ct)
    {
        var dto = new NotificationRuleDto(eventType, request.Channels, request.IsActive, Array.Empty<string>());
        var result = await _ruleService.UpsertRuleAsync(dto, ct);
        return Ok(result);
    }

    [Permission(
        resourceKey: "notification-rule",
        action: "delete",
        description: "Delete notification rule",
        recommendedRoles: new[] { "Admin", "User" }
    )]
    [HttpDelete("{eventType}")]
    public async Task<IActionResult> Delete(string eventType, CancellationToken ct) =>
        await _ruleService.DeleteRuleAsync(eventType, ct) ? NoContent() : NotFound();
}

public record UpsertNotificationRuleRequest(int Channels, bool IsActive);