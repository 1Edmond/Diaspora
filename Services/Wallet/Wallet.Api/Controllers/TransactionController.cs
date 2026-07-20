using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Interfaces;
using Shared.Application.Models;
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
[Route("api/wallet/transactions")]
[Authorize]
public sealed class TransactionController(
    IMessageBus _messageBus) : ControllerBase
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
        var command = new ProcedurePaymentCommand(
            request.ExternalProfileId,
            request.ProcedureId,
            request.Amount,
            request.Description);

        var result = await _messageBus.InvokeAsync<Result>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    [Permission("wallet", "pay-marketplace", "Pay for a marketplace service",
        recommendedRoles: new[] { "User" })]
    [HttpPost("marketplace-payment")]
    public async Task<IActionResult> PayMarketplace(
        [FromBody] MarketplacePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarketplacePaymentCommand(
            request.BuyerExternalProfileId,
            request.SellerExternalProfileId,
            request.ServiceId,
            request.Amount,
            request.Description);

        var result = await _messageBus.InvokeAsync<Result>(
            command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }
}
