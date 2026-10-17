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
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus,
    IServiceListingRepository? _listingRepository = null)
{
    public async Task<Result> Handle(CancelServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.RequesterId != command.RequestedBy)
            return Result.Failure("Only the requester can cancel a request.");

        var wasAccepted = request.Status == ServiceRequestStatus.Accepted;

        var cancelResult = request.Cancel(command.Reason);
        if (cancelResult.IsFailure)
            return cancelResult;

        if (wasAccepted && request.WeightKg.HasValue && request.WeightKg.Value > 0 && _listingRepository is not null)
        {
            var listing = await _listingRepository.GetByIdAsync(request.ListingId);
            if (listing is not null)
            {
                listing.ReleaseCapacity(request.WeightKg.Value);
                await _listingRepository.UpdateAsync(listing);
            }
        }

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            await _messageBus.PublishAsync(
                new MarketplacePaymentRefundRequestedIntegrationEvent(
                    request.Id,
                    BuyerProfileId: request.RequesterId,
                    Amount: request.AgreedPrice ?? 0m,
                    Currency: request.AgreedCurrency ?? "XOF"));
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