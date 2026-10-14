using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Security.Attributes;
using Wallet.Api.Security;
using Wallet.Application.Commands;
using Wallet.Application.Dtos;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;
using Wallet.Application.Requests;
using Wallet.Application.Responses;
using Wolverine;

namespace Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class WalletController(
    IMessageBus _messageBus,
    IWalletRepository _walletRepository,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission("wallet", "read", "Get wallet by external profile",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("profile/{externalProfileId:guid}")]
    public async Task<IActionResult> GetByProfile(
        Guid externalProfileId,
        CancellationToken cancellationToken)
    {
        if (!await WalletOwnership.CanReadAsync(
                _walletRepository, _currentUserService, externalProfileId, cancellationToken))
            return AccessDenied();

        var result = await _messageBus.InvokeAsync<Result<WalletDto>>(
            new GetWalletByProfileQuery(externalProfileId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission("wallet", "deposit", "Deposit funds into a wallet (admin only)",
        recommendedRoles: new[] { "Admin" })]
    [HttpPost("profile/{externalProfileId:guid}/deposit")]
    public async Task<IActionResult> Deposit(
        Guid externalProfileId,
        [FromBody] DepositRequest request,
        CancellationToken cancellationToken)
    {
        // Défense en profondeur : la permission « deposit » est réservée aux administrateurs,
        // mais on ne dépend pas uniquement de la configuration dynamique des permissions.
        if (!_currentUserService.IsInRole("Admin"))
            return AccessDenied();

        var command = new DepositCommand(
            externalProfileId, request.Amount, request.Description);

        var result = await _messageBus.InvokeAsync<Result<DepositResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission("wallet", "withdraw", "Request a withdrawal from a wallet",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpPost("profile/{externalProfileId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(
        Guid externalProfileId,
        [FromBody] WithdrawRequest request,
        CancellationToken cancellationToken)
    {
        // Un retrait ne peut être demandé que par le propriétaire du wallet (jamais par un admin
        // « à la place » de l'utilisateur).
        if (!await WalletOwnership.IsOwnerAsync(
                _walletRepository, _currentUserService, externalProfileId, cancellationToken))
            return AccessDenied();

        var command = new WithdrawCommand(
            externalProfileId, request.Amount, request.Description);

        var result = await _messageBus.InvokeAsync<Result<WithdrawResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    private IActionResult AccessDenied()
        => StatusCode(StatusCodes.Status403Forbidden, new { error = "Access to this wallet is denied." });
}
