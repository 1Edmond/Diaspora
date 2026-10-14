using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Interfaces;
using Wolverine;

namespace Wallet.Infrastructure.Consumers;

public sealed class MarketplacePaymentRefundRequestedConsumer(
    IWalletRepository _walletRepository,
    ITransactionRepository _transactionRepository,
    IMessageBus _messageBus,
    ILogger<MarketplacePaymentRefundRequestedConsumer> _logger)
{
    public async Task Handle(
        MarketplacePaymentRefundRequestedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[WalletService] Processing MarketplacePaymentRefundRequested for ServiceRequest {ServiceRequestId}",
            @event.ServiceRequestId);

        Domain.Entities.Wallet? buyer = null;

        if (@event.BuyerProfileId.HasValue)
        {
            buyer = await _walletRepository.GetByExternalProfileIdAsync(
                @event.BuyerProfileId.Value, cancellationToken);
        }

        var holdTx = await _transactionRepository.GetHoldTransactionByReferenceIdAsync(
            @event.ServiceRequestId, cancellationToken);

        if (buyer is null && holdTx is not null)
        {
            buyer = await _walletRepository.GetByIdAsync(holdTx.WalletId, cancellationToken);
        }

        if (buyer is null)
        {
            _logger.LogWarning(
                "[WalletService] Buyer wallet could not be resolved for refund of ServiceRequest {ServiceRequestId}",
                @event.ServiceRequestId);
            return;
        }

        var refundResult = buyer.RefundMarketplaceHold(
            @event.ServiceRequestId,
            holdTx?.Amount ?? 0m,
            $"Marketplace escrow refund for request {@event.ServiceRequestId}");

        if (refundResult.IsFailure)
        {
            _logger.LogWarning(
                "[WalletService] Refund hold failed for ServiceRequest {ServiceRequestId}: {Error}",
                @event.ServiceRequestId, refundResult.Error);
            return;
        }

        await _walletRepository.UpdateAsync(buyer, cancellationToken);

        _logger.LogInformation(
            "[WalletService] Escrow refunded (Tx: {TransactionId}) for ServiceRequest {ServiceRequestId}. Publishing MarketplacePaymentRefunded.",
            refundResult.Value.Id, @event.ServiceRequestId);

        await _messageBus.PublishAsync(new MarketplacePaymentRefundedIntegrationEvent(
            @event.ServiceRequestId, refundResult.Value.Id));
    }
}
