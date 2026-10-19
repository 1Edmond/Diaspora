using Freelance.Application.Commands;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Freelance.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/job-postings")]
public class JobPostingController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService,
    ILogger<JobPostingController> _logger) : ControllerBase
{
    // Public — consultation libre sans authentification
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] PaymentType? paymentType = null,
        [FromQuery] PaymentTiming? paymentTiming = null,
        [FromQuery] JobPostingStatus? status = null,
        [FromQuery] DateTime? eventStartFrom = null,
        [FromQuery] DateTime? eventStartTo = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] double? userLatitude = null,
        [FromQuery] double? userLongitude = null,
        [FromQuery] double? maxDistanceKm = null,
        [FromQuery] bool? isRemote = null,
        [FromQuery] IEnumerable<string>? requiredSkills = null,
        [FromQuery] bool activeOnly = true)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = new GetJobPostingsQuery(
            page, pageSize, categoryId, search, paymentType, paymentTiming, status,
            eventStartFrom, eventStartTo, city, country, userLatitude, userLongitude,
            maxDistanceKm, isRemote, requiredSkills, activeOnly);

        var result = await _messageBus.InvokeAsync<Result<PagedList<JobPostingSummaryDto>>>(query);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<JobPostingDto>>(
            new GetJobPostingByIdQuery(id));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "read-my",
        description: "View own job postings",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyListings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobPostingSummaryDto>>>(
            new GetMyJobPostingsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "create",
        description: "Create a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobPostingCommand command)
    {
        var cmd = command with { EmployerId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "update",
        description: "Update own job posting (draft only)",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobPostingCommand command)
    {
        var cmd = command with { Id = id, RequestedBy = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "delete",
        description: "Delete own job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteJobPostingCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "publish",
        description: "Publish a draft job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new PublishJobPostingCommand(id, GetCurrentUserId()));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Job posting published successfully" });
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "close-registration",
        description: "Close registration for a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/close-registration")]
    public async Task<IActionResult> CloseRegistration(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new CloseJobPostingRegistrationCommand(id, GetCurrentUserId()));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Registration closed" });
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "start",
        description: "Start a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new StartJobPostingCommand(id, GetCurrentUserId()));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Job posting started" });
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "complete",
        description: "Complete a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new CompleteJobPostingCommand(id, GetCurrentUserId()));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Job posting completed" });
    }

    [Permission(
        resourceKey: "freelance.job-posting",
        action: "cancel",
        description: "Cancel a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(new CancelJobPostingCommand(id, GetCurrentUserId()));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Job posting cancelled" });
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}