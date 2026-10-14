using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Consumers;

public sealed class MarketplacePaymentFailedConsumer(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    ILogger<MarketplacePaymentFailedConsumer> _logger)
{
    public async Task Handle(MarketplacePaymentFailedIntegrationEvent @event)
    {
        _logger.LogWarning(
            "[MarketplaceService] Received MarketplacePaymentFailed — ServiceRequestId: {ServiceRequestId}, Reason: {Reason}",
            @event.ServiceRequestId, @event.Reason);

        var request = await _requestRepository.GetByIdAsync(@event.ServiceRequestId);
        if (request is null)
        {
            _logger.LogWarning(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} not found, ignoring payment failed event",
                @event.ServiceRequestId);
            return;
        }

        if (request.PaymentStatus == PaymentStatus.Failed)
        {
            _logger.LogInformation(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} is already in Failed status (idempotent)",
                @event.ServiceRequestId);
            return;
        }

        request.MarkPaymentFailed();
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {ServiceRequestId} payment status updated to Failed",
            @event.ServiceRequestId);
    }
}
