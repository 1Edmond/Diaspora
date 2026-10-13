using Freelance.Application.Commands;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/job-check-ins")]
public class JobCheckInController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.job-check-in",
        action: "read",
        description: "View own job check-ins",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetMyCheckIns([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobCheckInDto>>>(
            new GetJobCheckInsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-check-in",
        action: "check-in",
        description: "Check in to a job",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        var command = new CheckInCommand(request.JobApplicationId, GetCurrentUserId(), request.Latitude, request.Longitude, request.Notes);
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "freelance.job-check-in",
        action: "check-out",
        description: "Check out from a job",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        var command = new CheckOutCommand(request.JobApplicationId, GetCurrentUserId(), request.Latitude, request.Longitude, request.Notes);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record CheckInRequest(Guid JobApplicationId, double? Latitude, double? Longitude, string? Notes);
public record CheckOutRequest(Guid JobApplicationId, double? Latitude, double? Longitude, string? Notes);