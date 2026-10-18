using Freelance.Application.Commands;
using Freelance.Application.Dtos;
using Freelance.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;

namespace Freelance.Api.Controllers;

[ApiController]
[Route("api/freelance/job-categories")]
public class JobCategoryController(
    IMessageBus _messageBus) : ControllerBase
{
    [Permission("FreelanceCategories.View", "Consulter les catégories freelance")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, [FromQuery] Guid? profileTypeId = null)
    {
        var result = await _messageBus.InvokeAsync<Result<List<JobCategoryDto>>>(
            new GetJobCategoriesQuery(activeOnly, profileTypeId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission("FreelanceCategories.Manage", "Gérer les catégories freelance")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission("FreelanceCategories.Manage", "Gérer les catégories freelance")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobCategoryCommand command, CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd, cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("FreelanceCategories.Manage", "Gérer les catégories freelance")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(new DeleteJobCategoryCommand(id), cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("FreelanceCategories.Manage", "Gérer les catégories freelance")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] bool isActive, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(new ToggleJobCategoryStatusCommand(id, isActive), cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission("FreelanceCategories.Manage", "Gérer les catégories freelance")]
    [HttpPut("{id:guid}/translations/{cultureCode}")]
    public IActionResult UpdateTranslation(Guid id, string cultureCode, [FromBody] Dictionary<string, string> fields)
    {
        return NoContent();
    }
}