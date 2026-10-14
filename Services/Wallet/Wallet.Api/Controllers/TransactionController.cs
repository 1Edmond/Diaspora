using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
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
[Route("api/wallet/transactions")]
[Authorize]
public sealed class TransactionController(
    IMessageBus _messageBus,
    IWalletRepository _walletRepository,
    ICurrentUserService _currentUserService) : ControllerBase
{
    [Permission("wallet", "read-transactions", "Get transaction history",
        recommendedRoles: new[] { "User", "Admin" })]
    [HttpGet("profile/{externalProfileId:guid}")]
    public async Task<IActionResult> GetTransactions(
        Guid externalProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!await WalletOwnership.CanReadAsync(
                _walletRepository, _currentUserService, externalProfileId, cancellationToken))
            return AccessDenied();

        var result = await _messageBus.InvokeAsync<Result<PagedList<TransactionDto>>>(
            new GetTransactionsQuery(externalProfileId, pageNumber, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission("wallet", "transfer", "Transfer funds to another member",
        recommendedRoles: new[] { "User" })]
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than zero." });

        if (request.SenderExternalProfileId == request.ReceiverExternalProfileId)
            return BadRequest(new { error = "Sender and receiver must be different." });

        // L'émetteur doit être le propriétaire authentifié du wallet débité.
        if (!await WalletOwnership.IsOwnerAsync(
                _walletRepository, _currentUserService, request.SenderExternalProfileId, cancellationToken))
            return AccessDenied();

        var command = new TransferCommand(
            request.SenderExternalProfileId,
            request.ReceiverExternalProfileId,
            request.Amount,
            request.Description);

        var result = await _messageBus.InvokeAsync<Result<TransferResponse>>(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [Permission("wallet", "pay-procedure", "Pay for an administrative procedure",
        recommendedRoles: new[] { "User" })]
    [HttpPost("procedure-payment")]
    public async Task<IActionResult> PayProcedure(
        [FromBody] ProcedurePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than zero." });

        if (!await WalletOwnership.IsOwnerAsync(
                _walletRepository, _currentUserService, request.ExternalProfileId, cancellationToken))
            return AccessDenied();

        var command = new ProcedurePaymentCommand(
            request.ExternalProfileId,
            request.ProcedureId,
            request.Amount,
            request.Description);

        var result = await _messageBus.InvokeAsync<Result<Guid>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    // ⚠ Endpoint transitoire (phase 1). Le paiement Marketplace sera remplacé en phase 2 par un flux
    // piloté par le Marketplace (séquestre) ; cet endpoint sera alors supprimé.
    // D'ici là : seul le propriétaire du wallet acheteur peut déclencher le paiement.
    [Permission("wallet", "pay-marketplace", "Pay for a marketplace service",
        recommendedRoles: new[] { "User" })]
    [HttpPost("marketplace-payment")]
    public async Task<IActionResult> PayMarketplace(
        [FromBody] MarketplacePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than zero." });

        if (request.BuyerExternalProfileId == request.SellerExternalProfileId)
            return BadRequest(new { error = "Buyer and seller must be different." });

        if (!await WalletOwnership.IsOwnerAsync(
                _walletRepository, _currentUserService, request.BuyerExternalProfileId, cancellationToken))
            return AccessDenied();

        var command = new MarketplacePaymentCommand(
            request.BuyerExternalProfileId,
            request.SellerExternalProfileId,
            request.ServiceId,
            request.Amount,
            request.Description);

        var result = await _messageBus.InvokeAsync<Result<Guid>>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    private IActionResult AccessDenied()
        => StatusCode(StatusCodes.Status403Forbidden, new { error = "Access to this wallet is denied." });
}
