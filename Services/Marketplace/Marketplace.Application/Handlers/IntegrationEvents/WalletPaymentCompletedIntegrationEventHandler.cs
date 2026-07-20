using Marketplace.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.IntegrationEvents;

public class WalletPaymentCompletedIntegrationEventHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    ILogger<WalletPaymentCompletedIntegrationEventHandler> _logger)
{
    public async Task Handle(MarketplacePaymentCompletedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "[MarketplaceService] Received MarketplacePaymentCompleted — ServiceId: {ServiceId}, TransactionId: {TransactionId}",
            @event.ServiceId,
            @event.TransactionId);

        var request = await _requestRepository.GetByIdAsync(@event.ServiceId);
        if (request is null)
        {
            _logger.LogWarning(
                "[MarketplaceService] ServiceRequest {ServiceId} not found, ignoring payment event",
                @event.ServiceId);
            return;
        }

        request.MarkPaymentCompleted(@event.TransactionId);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {ServiceId} payment marked as completed (TransactionId: {TransactionId})",
            @event.ServiceId,
            @event.TransactionId);
    }
}

