using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Consumers;

public sealed class MarketplacePaymentRefundedConsumer(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    ILogger<MarketplacePaymentRefundedConsumer> _logger)
{
    public async Task Handle(MarketplacePaymentRefundedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "[MarketplaceService] Received MarketplacePaymentRefunded — ServiceRequestId: {ServiceRequestId}, TransactionId: {TransactionId}",
            @event.ServiceRequestId, @event.TransactionId);

        var request = await _requestRepository.GetByIdAsync(@event.ServiceRequestId);
        if (request is null)
        {
            _logger.LogWarning(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} not found, ignoring payment refunded event",
                @event.ServiceRequestId);
            return;
        }

        if (request.PaymentStatus == PaymentStatus.Refunded)
        {
            _logger.LogInformation(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} is already in Refunded status (idempotent)",
                @event.ServiceRequestId);
            return;
        }

        request.MarkPaymentRefunded(@event.TransactionId);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {ServiceRequestId} payment status updated to Refunded",
            @event.ServiceRequestId);
    }
}
