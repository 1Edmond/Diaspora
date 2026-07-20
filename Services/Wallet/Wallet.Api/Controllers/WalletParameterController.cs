using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Requests;
using Wolverine;

namespace Wallet.Api.Controllers;

[ApiController]
[Route("api/wallet/parameters")]
[Authorize]
public sealed class WalletParameterController(
    IMessageBus _messageBus,
    IWalletParameterRepository _repository) : ControllerBase
{
    [Permission("wallet", "manage-parameters", "View wallet configuration parameters",
        recommendedRoles: new[] { "Admin" })]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var parameters = await _repository.GetAllAsync(cancellationToken);
        var dtos = parameters.Select(p => new { p.Key, p.Value, p.Description });
        return Ok(dtos);
    }

    [Permission("wallet", "manage-parameters", "Update a wallet configuration parameter",
        recommendedRoles: new[] { "Admin" })]
    [HttpPut("{key}")]
    public async Task<IActionResult> Update(
        string key,
        [FromBody] UpdateParameterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result>(
            new UpdateWalletParameterCommand(key, request.Value), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}


