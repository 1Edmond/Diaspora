using Microsoft.AspNetCore.Mvc;
using Profile.Application.Commands;
using Profile.Application.DTOs;
using Profile.Application.Queries;
using Profile.Application.Responses.Commands;
using Profile.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Profile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalProfileController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<ExternalProfileController> _logger) : ControllerBase
{
    [Permission(
        resourceKey: "profile.external",
        action: "create",
        description: "Create an external profile in a host country",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExternalProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExternalProfileCommand(
            GetCurrentUserId(),
            request.ExternalProfileTypeId,
            request.Kind);

        var result = await _messageBus.InvokeAsync<Result<CreateExternalProfileResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.external",
        action: "read-own",
        description: "Get all profiles (internal + external) of the current user",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfiles(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _messageBus.InvokeAsync<Result<UserProfilesDto>>(
            new GetUserProfilesQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.external",
        action: "read",
        description: "Get a specific external profile by ID",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{profileId:guid}")]
    public async Task<IActionResult> GetById(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<ExternalProfileDto>>(
            new GetExternalProfileByIdQuery(profileId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.external",
        action: "change-kind",
        description: "Change external profile kind",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPatch("{profileId:guid}/kind")]
    public async Task<IActionResult> ChangeKind(
        Guid profileId,
        [FromBody] ChangeKindRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeExternalProfileKindCommand(
            profileId,
            GetCurrentUserId(),
            request.NewKind);

        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile kind updated successfully" });
    }

    [Permission(
        resourceKey: "profile.external",
        action: "suspend",
        description: "Suspend an external profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/suspend")]
    public async Task<IActionResult> Suspend(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new SuspendExternalProfileCommand(profileId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile suspended successfully" });
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

