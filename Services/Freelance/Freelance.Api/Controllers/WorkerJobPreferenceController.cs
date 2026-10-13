using Freelance.Application.Commands;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/worker-preferences")]
public class WorkerJobPreferenceController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.worker-preference",
        action: "read",
        description: "View own job preferences",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetMyPreferences()
    {
        var result = await _messageBus.InvokeAsync<Result<WorkerJobPreferenceDto>>(
            new GetMyJobPreferencesQuery(GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.worker-preference",
        action: "update",
        description: "Set or update job preferences",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut]
    public async Task<IActionResult> SetPreferences([FromBody] SetWorkerJobPreferenceRequest request)
    {
        var command = new SetWorkerJobPreferenceCommand(
            GetCurrentUserId(),
            request.CategoryIds,
            request.City,
            request.Country,
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            request.OnlyRemote,
            request.EmailNotifications);

        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "freelance.worker-preference",
        action: "delete",
        description: "Remove job preferences",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete]
    public async Task<IActionResult> RemovePreferences()
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new RemoveWorkerJobPreferenceCommand(GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record SetWorkerJobPreferenceRequest(
    IEnumerable<Guid>? CategoryIds = null,
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null,
    int? RadiusKm = null,
    bool? OnlyRemote = null,
    bool? EmailNotifications = null
);