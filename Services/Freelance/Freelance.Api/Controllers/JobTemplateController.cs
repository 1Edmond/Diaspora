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
[Route("api/freelance/job-templates")]
public class JobTemplateController(
    IMessageBus _messageBus,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission(
        resourceKey: "freelance.job-template",
        action: "read-my",
        description: "View own job templates",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet]
    public async Task<IActionResult> GetMyTemplates([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobTemplateDto>>>(
            new GetJobTemplatesQuery(GetCurrentUserId(), page, pageSize));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-template",
        action: "read-admin",
        description: "View all job templates as admin",
        recommendedRoles: new[] { "Admin", "SuperAdmin" }
    )]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var result = await _messageBus.InvokeAsync<Result<PagedList<JobTemplateDto>>>(
            new GetJobTemplatesQuery(Guid.Empty, page, pageSize, IsAdmin: true));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "freelance.job-template",
        action: "create",
        description: "Create a job template",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobTemplateCommand command)
    {
        var cmd = command with { EmployerId = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result<Guid>>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetMyTemplates), new { id = result.Value }, new { id = result.Value });
    }

    [Permission(
        resourceKey: "freelance.job-template",
        action: "update",
        description: "Update own job template",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobTemplateCommand command)
    {
        var cmd = command with { Id = id, RequestedBy = GetCurrentUserId() };
        var result = await _messageBus.InvokeAsync<Result>(cmd);

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "freelance.job-template",
        action: "delete",
        description: "Delete own job template",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new DeleteJobTemplateCommand(id, GetCurrentUserId()));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    private Guid GetCurrentUserId() => Guid.Parse(_currentUserService.UserId!);
}