using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Security.Attributes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/marketplace/parameters")]
[Authorize]
public sealed class MarketplaceParameterController(
    IMarketplaceParameterRepository _repository) : ControllerBase
{
    [Permission(resourceKey: "marketplace.parameter", action: "read", description: "Consulter les paramètres marketplace", recommendedRoles: new[] { "Admin" })]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var parameters = await _repository.GetAllAsync(cancellationToken);
        var dtos = parameters.Select(p => new { p.Key, p.Value, p.Description, p.UpdatedAt });
        return Ok(dtos);
    }

    [Permission(resourceKey: "marketplace.parameter", action: "manage", description: "Modifier les paramètres marketplace", recommendedRoles: new[] { "Admin" })]
    [HttpPut("{key}")]
    public async Task<IActionResult> Update(
        string key,
        [FromBody] UpdateMarketplaceParameterRequest request,
        CancellationToken cancellationToken)
    {
        var param = await _repository.GetByKeyAsync(key, cancellationToken);
        if (param == null)
            return NotFound(new { error = $"Parameter '{key}' not found." });

        param.Update(request.Value);
        await _repository.UpdateAsync(param, cancellationToken);

        return NoContent();
    }
}

public record UpdateMarketplaceParameterRequest(string Value);
