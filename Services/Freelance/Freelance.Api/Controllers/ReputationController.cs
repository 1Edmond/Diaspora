using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/reputation")]
public class ReputationController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.reputation",
        action: "read",
        description: "View reputation",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyReputation()
    {
        var result = await _messageBus.InvokeAsync<Result<ReputationDto>>(
            new GetReputationQuery(GetCurrentUserId(), ReputationRole.Worker));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.reputation",
        action: "read",
        description: "View reputation as employer",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("me/employer")]
    public async Task<IActionResult> GetMyEmployerReputation()
    {
        var result = await _messageBus.InvokeAsync<Result<ReputationDto>>(
            new GetReputationQuery(GetCurrentUserId(), ReputationRole.Employer));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.reputation",
        action: "read",
        description: "View reputation of a user",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{subjectId:guid}")]
    public async Task<IActionResult> GetReputation(Guid subjectId, [FromQuery] ReputationRole role = ReputationRole.Worker)
    {
        var result = await _messageBus.InvokeAsync<Result<ReputationDto>>(
            new GetReputationQuery(subjectId, role));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}