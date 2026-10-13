using Microsoft.AspNetCore.Mvc;
using Shared.Application.Commands;
using Shared.Application.Dtos;
using Shared.Application.Queries;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wolverine;

namespace Shared.Web.Controllers;

[ApiController]
public abstract class BaseTranslatableController<TOwner, TEntityDto> : ControllerBase
    where TOwner : class, Shared.Domain.Interfaces.ITranslatable
{
    private readonly IMessageBus _messageBus;

    protected BaseTranslatableController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [Permission(
        resourceKey: "translatable",
        action: "create-with-translations",
        description: "Create entity with translations",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("with-translations")]
    public virtual async Task<IActionResult> CreateWithTranslations(
        [FromBody] CreateWithTranslationsCommand<TOwner, TEntityDto> command,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<Guid>>(command, cancellationToken);
        return result.IsFailure
            ? BadRequest(new { error = result.Error })
            : CreatedAtAction(nameof(GetTranslations), new { id = result.Value }, new { id = result.Value });
    }

    [Permission(
        resourceKey: "translatable",
        action: "update-with-translations",
        description: "Update entity with translations",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPut("{id:guid}/with-translations")]
    public virtual async Task<IActionResult> UpdateWithTranslations(
        Guid id,
        [FromBody] UpdateWithTranslationsCommand<TOwner, TEntityDto> command,
        CancellationToken cancellationToken)
    {
        var cmd = command with { EntityId = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd, cancellationToken);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [Permission(
        resourceKey: "translatable",
        action: "set-translation",
        description: "Set a single translation",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpPost("{id:guid}/translations")]
    public virtual async Task<IActionResult> SetTranslation(
        Guid id,
        [FromBody] SetTranslationCommand<TOwner> command,
        CancellationToken cancellationToken)
    {
        var cmd = command with { EntityId = id };
        var result = await _messageBus.InvokeAsync<Result>(cmd, cancellationToken);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [Permission(
        resourceKey: "translatable",
        action: "delete-translation",
        description: "Delete a translation",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpDelete("{id:guid}/translations/{culture}")]
    public virtual async Task<IActionResult> DeleteTranslation(
        Guid id,
        string culture,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTranslationCommand<TOwner>(id, culture);
        var result = await _messageBus.InvokeAsync<Result>(command, cancellationToken);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [Permission(
        resourceKey: "translatable",
        action: "get-translations",
        description: "Get all translations for entity",
        recommendedRoles: new[] { "User", "Admin" }
    )]
    [HttpGet("{id:guid}/translations")]
    public virtual async Task<IActionResult> GetTranslations(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTranslationsQuery<TOwner>(id);
        var result = await _messageBus.InvokeAsync<Result<IReadOnlyCollection<TranslationDto>>>(query, cancellationToken);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}