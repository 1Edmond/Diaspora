using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Interfaces;
using Wolverine;

namespace Wallet.Infrastructure.Consumers;

public sealed class MarketplacePaymentRequestedConsumer(
    IWalletRepository _walletRepository,
    IMessageBus _messageBus,
    ILogger<MarketplacePaymentRequestedConsumer> _logger)
{
    public async Task Handle(
        MarketplacePaymentRequestedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[WalletService] Processing MarketplacePaymentRequested for ServiceRequest {ServiceRequestId}, Amount: {Amount} {Currency}, BuyerProfile: {BuyerProfileId}",
            @event.ServiceRequestId, @event.Amount, @event.Currency, @event.BuyerProfileId);

        var buyer = await _walletRepository.GetByExternalProfileIdAsync(
            @event.BuyerProfileId, cancellationToken);

        if (buyer is null)
        {
            _logger.LogWarning(
                "[WalletService] Buyer wallet not found for profile {BuyerProfileId}. Failing payment.",
                @event.BuyerProfileId);
            await _messageBus.PublishAsync(new MarketplacePaymentFailedIntegrationEvent(
                @event.ServiceRequestId, "Buyer wallet not found."));
            return;
        }

        if (!buyer.IsActive)
        {
            _logger.LogWarning(
                "[WalletService] Buyer wallet {WalletId} is not active. Failing payment.", buyer.Id);
            await _messageBus.PublishAsync(new MarketplacePaymentFailedIntegrationEvent(
                @event.ServiceRequestId, "Buyer wallet is not active."));
            return;
        }

        if (!string.Equals(buyer.Currency, @event.Currency, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "[WalletService] Currency mismatch for buyer {WalletId}. Wallet: {WalletCurrency}, Requested: {RequestedCurrency}",
                buyer.Id, buyer.Currency, @event.Currency);
            await _messageBus.PublishAsync(new MarketplacePaymentFailedIntegrationEvent(
                @event.ServiceRequestId,
                $"Currency mismatch. Wallet currency: {buyer.Currency}, requested: {@event.Currency}."));
            return;
        }

        if (buyer.Balance < @event.Amount)
        {
            _logger.LogWarning(
                "[WalletService] Insufficient balance for buyer {WalletId}. Balance: {Balance}, Required: {Required}",
                buyer.Id, buyer.Balance, @event.Amount);
            await _messageBus.PublishAsync(new MarketplacePaymentFailedIntegrationEvent(
                @event.ServiceRequestId,
                $"Insufficient balance. Required: {@event.Amount} {@event.Currency}, Available: {buyer.Balance} {buyer.Currency}."));
            return;
        }

        var holdResult = buyer.HoldForMarketplace(
            @event.ServiceRequestId,
            @event.Amount,
            @event.Currency,
            $"Marketplace escrow hold for request {@event.ServiceRequestId}",
            @event.SellerProfileId);

        if (holdResult.IsFailure)
        {
            _logger.LogWarning(
                "[WalletService] Failed to place escrow hold for request {ServiceRequestId}: {Error}",
                @event.ServiceRequestId, holdResult.Error);
            await _messageBus.PublishAsync(new MarketplacePaymentFailedIntegrationEvent(
                @event.ServiceRequestId, holdResult.Error));
            return;
        }

        await _walletRepository.UpdateAsync(buyer, cancellationToken);

        _logger.LogInformation(
            "[WalletService] Escrow hold created (Tx: {TransactionId}) for ServiceRequest {ServiceRequestId}. Publishing MarketplacePaymentHeld.",
            holdResult.Value.Id, @event.ServiceRequestId);

        await _messageBus.PublishAsync(new MarketplacePaymentHeldIntegrationEvent(
            @event.ServiceRequestId, holdResult.Value.Id));
    }
}
