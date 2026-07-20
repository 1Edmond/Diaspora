using Marketplace.Application.Commands;
using Marketplace.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(IMessageBus _messageBus, ILogger<CategoryController> _logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var result = await _messageBus.InvokeAsync<Result<List<Application.Dtos.CategoryDto>>>(
            new GetAllCategoriesQuery(activeOnly));

        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

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
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
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
}
