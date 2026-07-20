using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class MarketplacePaymentCommandHandler(IWalletRepository repository)
{
    public async Task<Result<Guid>> Handle(
        MarketplacePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var buyer = await repository.GetByExternalProfileIdAsync(
            command.BuyerExternalProfileId, cancellationToken);
        if (buyer is null)
            return Result.Failure<Guid>("Buyer wallet not found.");

        var seller = await repository.GetByExternalProfileIdAsync(
            command.SellerExternalProfileId, cancellationToken);
        if (seller is null)
            return Result.Failure<Guid>("Seller wallet not found.");

        var debitResult = buyer.DebitForMarketplace(
            command.Amount, command.ServiceId, seller.Id, command.Description);

        if (debitResult.IsFailure)
            return Result.Failure<Guid>(debitResult.Error);

        var creditResult = seller.CreditFromMarketplace(
            command.Amount, command.ServiceId, command.Description);

        if (creditResult.IsFailure)
            return Result.Failure<Guid>(creditResult.Error);

        buyer.AddDomainEvent(new MarketplacePaymentCompletedDomainEvent(
            buyer.Id, seller.Id, debitResult.Value.Id,
            command.ServiceId, command.Amount, buyer.Currency));

        await repository.UpdateAsync(buyer, cancellationToken);
        await repository.UpdateAsync(seller, cancellationToken);

        return Result.Success(debitResult.Value.Id);
    }
}

