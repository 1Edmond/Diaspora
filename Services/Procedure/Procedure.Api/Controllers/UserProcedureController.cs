using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Procedure.Application.Commands;
using Procedure.Application.DTOs;
using Procedure.Application.Queries;
using Procedure.Application.Requests;
using Procedure.Application.Responses;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Security.Attributes;
using Wolverine;

namespace Procedure.Api.Controllers;

[ApiController]
[Route("api/user-procedures")]
[Authorize]
public class UserProcedureController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<UserProcedureController> _logger) : ControllerBase
{
    private bool IsAdminOrOfficer() =>
        _currentUserService.IsInRole("Admin")
        || _currentUserService.IsInRole("SuperAdmin")
        || _currentUserService.IsInRole("Officer")
        || User.IsInRole("Admin")
        || User.IsInRole("SuperAdmin")
        || User.IsInRole("Officer");

    [Permission(
        resourceKey: "procedure.user",
        action: "list-admin",
        description: "Get all user procedures across profiles for administration",
        recommendedRoles: new[] { "Admin", "Officer" }
    )]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin(
        [FromQuery] UserProcedureStatus? status = null,
        [FromQuery] Guid? procedureId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdminOrOfficer())
            return Forbid();

        var result = await _messageBus.InvokeAsync<Result<PagedList<UserProcedureDto>>>(
            new GetAdminUserProceduresQuery(status, procedureId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure.user",
        action: "list",
        description: "Get all user procedures for a profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("profile/{profileId:guid}")]
    public async Task<IActionResult> GetByProfile(
        Guid profileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
            return Unauthorized();

        var result = await _messageBus.InvokeAsync<Result<PagedList<UserProcedureDto>>>(
            new GetUserProceduresQuery(profileId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure.user",
        action: "progress",
        description: "Get procedure completion progress for a profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("profile/{profileId:guid}/progress")]
    public async Task<IActionResult> GetProgress(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
            return Unauthorized();

        var result = await _messageBus.InvokeAsync<Result<ProcedureProgressDto>>(
            new GetProcedureProgressQuery(profileId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure.user",
        action: "start",
        description: "Start a procedure for the current user profile",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("start")]
    public async Task<IActionResult> Start(
        [FromBody] StartUserProcedureRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
            return Unauthorized();

        var command = new StartUserProcedureCommand(request.ProfileId, request.ProcedureId);

        var result = await _messageBus.InvokeAsync<Result<StartUserProcedureResponse>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "procedure.user",
        action: "complete",
        description: "Mark a procedure as completed",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{userProcedureId:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid userProcedureId,
        [FromBody] CompleteUserProcedureRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
            return Unauthorized();

        var role = IsAdminOrOfficer()
            ? "Admin"
            : "User";

        var command = new CompleteUserProcedureCommand(
            userProcedureId,
            _currentUserService.UserId!,
            role,
            request.Notes);

        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Procedure completed successfully" });
    }

    [Permission(
        resourceKey: "procedure.user",
        action: "skip",
        description: "Skip a procedure",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{userProcedureId:guid}/skip")]
    public async Task<IActionResult> Skip(
        Guid userProcedureId,
        [FromBody] SkipUserProcedureRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.UserId))
            return Unauthorized();

        var result = await _messageBus.InvokeAsync<Result>(
            new SkipUserProcedureCommand(userProcedureId, request.Notes),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Procedure skipped" });
    }
}



