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
[Route("api/freelance/job-reviews")]
public class JobReviewController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.job-review",
        action: "read",
        description: "View job reviews for an application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("application/{jobApplicationId:guid}")]
    public async Task<IActionResult> GetByApplication(Guid jobApplicationId)
    {
        // For now return all reviews for the application
        // In a real scenario, you'd add a specific query for this
        return Ok(new { message = "Use GetReputation endpoint for aggregated reviews" });
    }

    [Permission(
        resourceKey: "freelance.job-review",
        action: "create",
        description: "Submit a job review",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobReviewCommand command)
    {
        var cmd = command with { RaterId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}