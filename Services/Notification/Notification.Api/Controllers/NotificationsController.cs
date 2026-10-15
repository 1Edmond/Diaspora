using Microsoft.AspNetCore.Mvc;
using Notification.Application.Abstractions;
using Notification.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Security.Attributes;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(
    INotificationQueryService _queryService,
    INotificationCommandService _commandService,
    ICurrentUserService _currentUserService) : ControllerBase
{
    // GET /notifications?pageNumber=1&pageSize=20&unreadOnly=false
    [Permission(
        resourceKey: "notification",
        action: "read",
        description: "List my notifications",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<ActionResult<PagedList<UserNotificationDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default) =>
        Ok(await _queryService.GetUserNotificationsAsync(GetUserId(), pageNumber, pageSize, unreadOnly, ct));

    [Permission(
        resourceKey: "notification",
        action: "read",
        description: "Get my unread notification count",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount(CancellationToken ct) =>
        Ok(new { count = await _queryService.GetUnreadCountAsync(GetUserId(), ct) });

    [Permission(
        resourceKey: "notification",
        action: "update",
        description: "Mark a notification as read",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct) =>
        await _commandService.MarkAsReadAsync(GetUserId(), id, ct) ? NoContent() : NotFound();

    [Permission(
        resourceKey: "notification",
        action: "update",
        description: "Mark all notifications as read",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct) =>
        Ok(new { updatedCount = await _commandService.MarkAllAsReadAsync(GetUserId(), ct) });

    [Permission(
        resourceKey: "notification",
        action: "broadcast",
        description: "Send broadcast campaign notification",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast(
        [FromBody] BroadcastNotificationRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Title and Message are required." });

        var campaignId = await _commandService.BroadcastNotificationAsync(
            request.Title,
            request.Message,
            request.Channel,
            request.TargetRole,
            request.ProfileTypeId,
            ct);

        return Ok(new { id = campaignId, message = "Broadcast campaign initiated successfully" });
    }

    private Guid GetUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record BroadcastNotificationRequest(
    string Title,
    string Message,
    string Channel = "InApp",
    string? TargetRole = null,
    Guid? ProfileTypeId = null
);

