using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Abstractions;
using Notification.Application.DTOs;
using Shared.Application.Models;
using System.Security.Claims;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _queryService;
    private readonly INotificationCommandService _commandService;

    public NotificationsController(INotificationQueryService q, INotificationCommandService c)
    {
        _queryService = q;
        _commandService = c;
    }

    // GET /notifications?pageNumber=1&pageSize=20&unreadOnly=false
    [HttpGet]
    public async Task<ActionResult<PagedList<UserNotificationDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default) =>
        Ok(await _queryService.GetUserNotificationsAsync(GetUserId(), pageNumber, pageSize, unreadOnly, ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount(CancellationToken ct) =>
        Ok(new { count = await _queryService.GetUnreadCountAsync(GetUserId(), ct) });

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct) =>
        await _commandService.MarkAsReadAsync(GetUserId(), id, ct) ? NoContent() : NotFound();

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct) =>
        Ok(new { updatedCount = await _commandService.MarkAllAsReadAsync(GetUserId(), ct) });

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);
}