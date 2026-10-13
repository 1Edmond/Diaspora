using Marketplace.Application.Commands;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using System;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "marketplace.report",
        action: "create",
        description: "Report a listing, review, or user for abusive behavior",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request)
    {
        var command = new CreateReportCommand(request.TargetType, request.TargetId, GetCurrentUserId(), request.Reason);
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "marketplace.report",
        action: "read-pending",
        description: "View pending reports for moderation",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<ReportDto>>>(
            new GetPendingReportsQuery(page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.report",
        action: "resolve",
        description: "Resolve a report",
        recommendedRoles: new[] { "Admin", "Moderator" }
    )]
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveReportRequest request)
    {
        var command = new ResolveReportCommand(id, GetCurrentUserId(), request.NewStatus, request.ResolutionNotes);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record CreateReportRequest(ReportTargetType TargetType, Guid TargetId, string Reason);
public record ResolveReportRequest(ReportStatus NewStatus, string? ResolutionNotes);