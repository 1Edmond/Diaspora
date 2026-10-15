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
[Route("api/profiles")]
public class ProfileController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<ProfileController> _logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProfileCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { UserId = GetCurrentUserId() };

        var result = await _messageBus.InvokeAsync<Result<CreateProfileResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
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
        var command = new ChangeProfileKindCommand(
            profileId,
            GetCurrentUserId(),
            request.NewKind);

        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile kind updated successfully" });
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "list",
        description: "List internal profiles",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetProfiles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? status = null,
        [FromQuery] Guid? profileTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var statusEnum = status.HasValue ? (Profile.Domain.Enums.ProfileStatus)status.Value : (Profile.Domain.Enums.ProfileStatus?)null;
        
        var query = new GetProfilesQuery(pageNumber, pageSize, searchTerm, statusEnum, profileTypeId);

        var result = await _messageBus.InvokeAsync<Result<PagedList<ProfileDto>>>(
            query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

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
        var query = new GetPendingProfilesQuery(pageNumber, pageSize);

        var result = await _messageBus.InvokeAsync<Result<PagedList<ProfileDto>>>(
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
            new VerifyProfileCommand(profileId, adminId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile verified successfully" });
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "verify",
        description: "Set verification level for a profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/verification-level")]
    public async Task<IActionResult> SetVerificationLevel(
        Guid profileId,
        [FromBody] SetProfileVerificationLevelRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId ?? "Admin";

        var result = await _messageBus.InvokeAsync<Result>(
            new SetProfileVerificationLevelCommand(profileId, request.Level, adminId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Verification level updated successfully" });
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "read",
        description: "Get a profile by id (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpGet("{profileId:guid}")]
    public async Task<IActionResult> GetById(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<ProfileDto>>(
            new GetProfileByIdQuery(profileId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "activate",
        description: "Activate a user profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ActivateProfileCommand(profileId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile activated successfully" });
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "activate",
        description: "Reactivate a suspended user profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new ActivateProfileCommand(profileId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile reactivated successfully" });
    }

    [Permission(
        resourceKey: "profile.internal",
        action: "suspend",
        description: "Suspend a user profile (admin only)",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("{profileId:guid}/suspend")]
    public async Task<IActionResult> Suspend(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new SuspendProfileCommand(profileId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Profile suspended successfully" });
    }

    [Permission(
        resourceKey: "profile",
        action: "admin",
        description: "Republier l'annuaire des profils",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost("admin/republish-directory")]
    public async Task<IActionResult> RepublishDirectory(CancellationToken cancellationToken)
    {
        var command = new RepublishProfileDirectoryCommand();
        var result = await _messageBus.InvokeAsync<Result<RepublishProfileDirectoryResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}
