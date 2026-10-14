using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Consumers;

public sealed class MarketplacePaymentReleasedConsumer(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    ILogger<MarketplacePaymentReleasedConsumer> _logger)
{
    public async Task Handle(MarketplacePaymentReleasedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "[MarketplaceService] Received MarketplacePaymentReleased — ServiceRequestId: {ServiceRequestId}, TransactionId: {TransactionId}",
            @event.ServiceRequestId, @event.TransactionId);

        var request = await _requestRepository.GetByIdAsync(@event.ServiceRequestId);
        if (request is null)
        {
            _logger.LogWarning(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} not found, ignoring payment released event",
                @event.ServiceRequestId);
            return;
        }

        if (request.PaymentStatus == PaymentStatus.Released)
        {
            _logger.LogInformation(
                "[MarketplaceService] ServiceRequest {ServiceRequestId} is already in Released status (idempotent)",
                @event.ServiceRequestId);
            return;
        }

        request.MarkPaymentReleased(@event.TransactionId);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {ServiceRequestId} payment status updated to Released",
            @event.ServiceRequestId);
    }
}
