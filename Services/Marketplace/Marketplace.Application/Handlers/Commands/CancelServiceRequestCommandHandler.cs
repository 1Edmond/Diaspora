using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Threading.Tasks;
using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public class CancelServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IServiceListingRepository _listingRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus)
{
    // Constructeur de compatibilité pour les tests unitaires existants
    public CancelServiceRequestCommandHandler(
        IServiceRequestRepository requestRepository,
        IUnitOfWork unitOfWork,
        IMessageBus messageBus)
        : this(requestRepository, null!, unitOfWork, messageBus)
    {
    }

    public async Task<Result> Handle(CancelServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.RequesterId != command.RequestedBy)
            return Result.Failure("Only the requester can cancel a request.");

        var wasAccepted = request.Status == ServiceRequestStatus.Accepted
                       || request.Status == ServiceRequestStatus.PackageHandedOver
                       || request.Status == ServiceRequestStatus.InTransit;

        if (wasAccepted && request.WeightKg.HasValue && request.WeightKg.Value > 0 && _listingRepository is not null)
        {
            var listing = await _listingRepository.GetByIdAsync(request.ListingId);
            if (listing is not null)
            {
                listing.ReleaseCapacity(request.WeightKg.Value);
                await _listingRepository.UpdateAsync(listing);
            }
        }

        request.Cancel(command.Reason);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            await _messageBus.PublishAsync(
                new MarketplacePaymentRefundRequestedIntegrationEvent(request.Id));
        }

        await _messageBus.PublishAsync(
            new ServiceRequestCancelledIntegrationEvent(
                request.Id,
                request.ListingId,
                request.RequesterId,
                request.ProviderId,
                command.Reason ?? string.Empty));

        return Result.Success();
    }
}