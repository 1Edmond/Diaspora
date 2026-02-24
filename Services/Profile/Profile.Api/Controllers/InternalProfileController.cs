using Microsoft.AspNetCore.Mvc;
using Profile.Application.Commands;
using Profile.Application.DTOs;
using Profile.Application.Queries;
using Profile.Application.Responses.Commands;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Profile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InternalProfileController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<InternalProfileController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "profile.internal",
        action: "create",
        description: "Create internal (Togo) profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInternalProfileCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { UserId = GetCurrentUserId() };

        var result = await _messageBus.InvokeAsync<Result<CreateInternalProfileResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "read-own",
        description: "Get own internal profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _messageBus.InvokeAsync<Result<InternalProfileDto>>(
            new GetInternalProfileByUserIdQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "list-pending",
        description: "List internal profiles pending admin verification",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingInternalProfilesQuery(pageNumber, pageSize);

        var result = await _messageBus.InvokeAsync<Result<PagedList<InternalProfileDto>>>(
            query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "verify",
        description: "Verify a user internal profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/verify")]
    public async Task<IActionResult> Verify(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId!;

        var result = await _messageBus.InvokeAsync<Result>(
            new VerifyInternalProfileCommand(profileId, adminId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile verified successfully" });
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}
