using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Abstractions;
using Shared.Security.Attributes;
using System.Security.Claims;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationStreamController : ControllerBase
{
    private readonly IConnectionManager _connectionManager;
    private readonly INotificationHistoryService _historyService;

    public NotificationStreamController(
        IConnectionManager connectionManager,
        INotificationHistoryService historyService)
    {
        _connectionManager = connectionManager;
        _historyService = historyService;
    }

    [HttpGet("stream")]
    [Authorize]
    [Permission(resourceKey: "notification.stream", action: "listen", description: "Écouter le flux SSE des notifications", recommendedRoles: new[] { "User", "Admin" })]
    public async Task StreamAsync(CancellationToken ct)
    {
        var userId = GetUserIdFromClaims();
        if (userId is null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, HttpContext.RequestAborted);
        var streamCt = linkedCts.Token;

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await _connectionManager.AddConnectionAsync(userId.Value, Response, streamCt);

        try
        {
            if (Request.Headers.TryGetValue("Last-Event-ID", out var lastEventIdHeader)
                && long.TryParse(lastEventIdHeader, out var lastSequence))
            {
                var missed = await _historyService.GetNotificationsAfterAsync(userId.Value, lastSequence, streamCt);
                foreach (var n in missed)
                    await _connectionManager.SendToUserAsync(userId.Value, n.Sequence, n.Id);
            }

            await Task.Delay(Timeout.Infinite, streamCt);
        }
        catch (OperationCanceledException)
        {
            // déconnexion normale du client
        }
        finally
        {
            _connectionManager.RemoveConnection(userId.Value);
        }
    }

    private Guid? GetUserIdFromClaims()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}