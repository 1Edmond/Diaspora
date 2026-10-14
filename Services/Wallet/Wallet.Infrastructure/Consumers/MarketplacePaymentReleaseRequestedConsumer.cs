using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Interfaces;
using Wolverine;

namespace Wallet.Infrastructure.Consumers;

public sealed class MarketplacePaymentReleaseRequestedConsumer(
    IWalletRepository _walletRepository,
    ITransactionRepository _transactionRepository,
    IMessageBus _messageBus,
    ILogger<MarketplacePaymentReleaseRequestedConsumer> _logger)
{
    public async Task Handle(
        MarketplacePaymentReleaseRequestedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[WalletService] Processing MarketplacePaymentReleaseRequested for ServiceRequest {ServiceRequestId}",
            @event.ServiceRequestId);

        Domain.Entities.Wallet? buyer = null;
        Domain.Entities.Wallet? seller = null;

        if (@event.BuyerProfileId.HasValue)
        {
            buyer = await _walletRepository.GetByExternalProfileIdAsync(
                @event.BuyerProfileId.Value, cancellationToken);
        }

        if (@event.SellerProfileId.HasValue)
        {
            seller = await _walletRepository.GetByExternalProfileIdAsync(
                @event.SellerProfileId.Value, cancellationToken);
        }

        var holdTx = await _transactionRepository.GetHoldTransactionByReferenceIdAsync(
            @event.ServiceRequestId, cancellationToken);

        if (buyer is null && holdTx is not null)
        {
            buyer = await _walletRepository.GetByIdAsync(holdTx.WalletId, cancellationToken);
        }

        if (seller is null && holdTx is not null && !string.IsNullOrWhiteSpace(holdTx.ReferenceType) && holdTx.ReferenceType.StartsWith("SellerProfile:"))
        {
            if (Guid.TryParse(holdTx.ReferenceType["SellerProfile:".Length..], out var sellerProfileId))
            {
                seller = await _walletRepository.GetByExternalProfileIdAsync(sellerProfileId, cancellationToken);
            }
        }

        if (buyer is null)
        {
            _logger.LogWarning(
                "[WalletService] Buyer wallet could not be resolved for release of ServiceRequest {ServiceRequestId}",
                @event.ServiceRequestId);
            return;
        }

        if (seller is null)
        {
            _logger.LogWarning(
                "[WalletService] Seller wallet could not be resolved for release of ServiceRequest {ServiceRequestId}",
                @event.ServiceRequestId);
            return;
        }

        var releaseResult = buyer.ReleaseMarketplaceHold(
            @event.ServiceRequestId,
            seller,
            holdTx?.Amount ?? 0m,
            $"Marketplace escrow release for request {@event.ServiceRequestId}");

        if (releaseResult.IsFailure)
        {
            _logger.LogWarning(
                "[WalletService] Release hold failed for ServiceRequest {ServiceRequestId}: {Error}",
                @event.ServiceRequestId, releaseResult.Error);
            return;
        }

        await _walletRepository.UpdateAsync(buyer, cancellationToken);
        await _walletRepository.UpdateAsync(seller, cancellationToken);

        _logger.LogInformation(
            "[WalletService] Escrow released (Tx: {TransactionId}) for ServiceRequest {ServiceRequestId}. Publishing MarketplacePaymentReleased.",
            releaseResult.Value.Id, @event.ServiceRequestId);

        await _messageBus.PublishAsync(new MarketplacePaymentReleasedIntegrationEvent(
            @event.ServiceRequestId, releaseResult.Value.Id));
    }
}
