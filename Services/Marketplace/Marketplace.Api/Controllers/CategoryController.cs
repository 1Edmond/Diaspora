using Marketplace.Application.Commands;
using Marketplace.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Security.Attributes;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(
    IMessageBus _messageBus,
    ILogger<CategoryController> _logger,
    ITranslationRepository<Marketplace.Domain.Entities.Category> _translationRepo,
    IUnitOfWork _unitOfWork) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, [FromQuery] Guid? profileTypeId = null)
    {
        var result = await _messageBus.InvokeAsync<Result<List<Application.Dtos.CategoryDto>>>(
            new GetAllCategoriesQuery(activeOnly, profileTypeId));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _messageBus.InvokeAsync<Result<Application.Dtos.CategoryDto>>(
            new GetCategoryByIdQuery(id));

        if (result.IsFailure) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [Permission(
        resourceKey: "marketplace.category",
        action: "create",
        description: "Create a service category",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Value });
    }

    [Permission(
        resourceKey: "marketplace.category",
        action: "update",
        description: "Update a service category",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        var cmd = command with { Id = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.category",
        action: "toggle-status",
        description: "Activate or deactivate a category",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] bool isActive)
    {
        var result = await _messageBus.InvokeAsync<Result>(new ToggleCategoryStatusCommand(id, isActive));
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [Permission(
        resourceKey: "marketplace.category",
        action: "update-policy",
        description: "Update a service category policy",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}/policy")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateCategoryPolicyCommand command)
    {
        var cmd = command with { CategoryId = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/translations")]
    public async Task<IActionResult> GetTranslations(Guid id, CancellationToken cancellationToken)
    {
        var translations = await _translationRepo.GetByOwnerIdAsync(id, cancellationToken);
        var result = translations.ToDictionary(
            t => t.CultureCode,
            t => t.Fields
        );
        return Ok(result);
    }

    [Permission(
        resourceKey: "marketplace.category",
        action: "update",
        description: "Update translations for a service category",
        recommendedRoles: new[] { "Admin" }
    )]
    [HttpPut("{id:guid}/translations/{cultureCode}")]
    public async Task<IActionResult> UpdateTranslation(
        Guid id,
        string cultureCode,
        [FromBody] Dictionary<string, string> fields,
        CancellationToken cancellationToken)
    {
        await _translationRepo.UpsertAsync(id, cultureCode, fields, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

