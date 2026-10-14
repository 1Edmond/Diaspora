using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Consumers;

public sealed class MarketplacePaymentHeldConsumer(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    ILogger<MarketplacePaymentHeldConsumer> _logger)
{
    public async Task Handle(MarketplacePaymentHeldIntegrationEvent @event)
    {
        _logger.LogInformation(
            "[MarketplaceService] Received MarketplacePaymentHeld — ServiceRequestId: {ServiceRequestId}, TransactionId: {TransactionId}",
            @event.ServiceRequestId, @event.TransactionId);

        var request = await _requestRepository.GetByIdAsync(@event.ServiceRequestId);
        if (request is null)
        {
            _logger.LogWarning(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} not found, ignoring payment held event",
                @event.ServiceRequestId);
            return;
        }

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            _logger.LogInformation(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} is already in Held status (idempotent)",
                @event.ServiceRequestId);
            return;
        }

        request.MarkPaymentHeld(@event.TransactionId);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {ServiceRequestId} payment status updated to Held",
            @event.ServiceRequestId);
    }
}
