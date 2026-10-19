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
    [Permission(resourceKey: "freelance.category", action: "read", description: "Consulter les catégories freelance", recommendedRoles: new[] { "Admin", "User", "Moderator" })]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, [FromQuery] Guid? profileTypeId = null)
    {
        var result = await _messageBus.InvokeAsync<Result<List<JobCategoryDto>>>(
            new GetJobCategoriesQuery(activeOnly, profileTypeId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(resourceKey: "freelance.category", action: "manage", description: "Créer une catégorie freelance", recommendedRoles: new[] { "Admin" })]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(resourceKey: "freelance.category", action: "manage", description: "Modifier une catégorie freelance", recommendedRoles: new[] { "Admin" })]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJobCategoryCommand command, CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd, cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(resourceKey: "freelance.category", action: "manage", description: "Supprimer une catégorie freelance", recommendedRoles: new[] { "Admin" })]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(new DeleteJobCategoryCommand(id), cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(resourceKey: "freelance.category", action: "manage", description: "Activer ou désactiver une catégorie freelance", recommendedRoles: new[] { "Admin" })]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] bool isActive, CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(new ToggleJobCategoryStatusCommand(id, isActive), cancellationToken);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(resourceKey: "freelance.category", action: "manage", description: "Gérer les traductions d'une catégorie freelance", recommendedRoles: new[] { "Admin" })]
    [HttpPut("{id:guid}/translations/{cultureCode}")]
    public IActionResult UpdateTranslation(Guid id, string cultureCode, [FromBody] Dictionary<string, string> fields)
    {
        return NoContent();
    }
}