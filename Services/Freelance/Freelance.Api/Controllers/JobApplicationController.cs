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
[Route("api/freelance/job-applications")]
public class JobApplicationController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.job-application",
        action: "read-sent",
        description: "View sent job applications",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobApplicationDto>>>(
            new GetMyJobApplicationsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "read-received",
        description: "View received job applications",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobApplicationDto>>>(
            new GetReceivedJobApplicationsQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "read-detail",
        description: "View a specific job application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<JobApplicationDto>>(
            new GetJobApplicationByIdQuery(id, GetCurrentUserId()));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "create",
        description: "Apply to a job posting",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApplyToJobCommand command)
    {
        var cmd = command with { WorkerId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "accept",
        description: "Accept a received job application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptApplicationRequest request)
    {
        var command = new AcceptApplicationCommand(id, GetCurrentUserId(), request.EscrowTransactionId);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Application accepted" });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "reject",
        description: "Reject a received job application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectApplicationRequest request)
    {
        var command = new RejectApplicationCommand(id, GetCurrentUserId(), request.Reason);
        var result = await _messageBus.InvokeAsync<Result>(command);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Application rejected" });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "withdraw",
        description: "Withdraw a sent job application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new WithdrawApplicationCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Application withdrawn" });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "complete",
        description: "Mark a job application as completed",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new CompleteApplicationCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Application completed" });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "mark-no-show",
        description: "Mark an accepted application as no-show",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new MarkApplicationNoShowCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { message = "Application marked as no-show" });
    }

    [Permission(
        resourceKey: "freelance.job-application",
        action: "link-chat-thread",
        description: "Link a chat thread to a job application",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/chat-thread")]
    public async Task<IActionResult> LinkChatThread(Guid id, [FromBody] LinkChatThreadRequest request)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new LinkApplicationChatThreadCommand(id, GetCurrentUserId(), request.ChatThreadId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}

public record AcceptApplicationRequest(Guid? EscrowTransactionId);
public record RejectApplicationRequest(string? Reason);
public record LinkChatThreadRequest(Guid ChatThreadId);