using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wallet.Application.Commands;
using Wallet.Application.Dtos;
using Wallet.Application.Queries;
using Wallet.Application.Requests;
using Wallet.Application.Responses;
using Wolverine;

namespace Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class WalletController(
    IMessageBus _messageBus) : ControllerBase
{
    [Permission("wallet", "read", "Get wallet by external profile",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("profile/{externalProfileId:guid}")]
    public async Task<IActionResult> GetByProfile(
        Guid externalProfileId,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.InvokeAsync<Result<WalletDto>>(
            new GetWalletByProfileQuery(externalProfileId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    //[Permission("wallet", "read", "Get wallet balance",
    //    recommendedRoles: new[] { "User", "Admin" })]
    //[HttpGet("profile/{externalProfileId:guid}/balance")]
    //public async Task<IActionResult> GetBalance(
    //    Guid externalProfileId,
    //    CancellationToken cancellationToken)
    //{
    //    var result = await _messageBus.InvokeAsync<Result<BalanceDto>>(
    //        new GetBalanceQuery(externalProfileId), cancellationToken);

    //    if (result.IsFailure)
    //        return NotFound(new { error = result.Error });

    //    return Ok(result.Value);
    //}

    [Permission("wallet", "deposit", "Deposit funds into a wallet (admin only)",
        recommendedRoles: new[] { "Admin" })]
    [HttpPost("profile/{externalProfileId:guid}/deposit")]
    public async Task<IActionResult> Deposit(
        Guid externalProfileId,
        [FromBody] DepositRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DepositCommand(
            externalProfileId, request.Amount, request.Description);

        var result = await _messageBus.InvokeAsync<Result<DepositResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}


