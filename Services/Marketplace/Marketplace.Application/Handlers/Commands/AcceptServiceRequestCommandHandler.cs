using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Shared.Messaging.Events.Marketplace;
using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public class AcceptServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IServiceListingRepository? _listingRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus? _messageBus = null)
{
    public async Task<Result> Handle(AcceptServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.ProviderId)
            return Result.Failure("You are not the provider for this request.");

        if (request.WeightKg.HasValue && request.WeightKg.Value > 0 && _listingRepository is not null)
        {
            var listing = await _listingRepository.GetByIdAsync(request.ListingId);
            if (listing is null)
                return Result.Failure("Associated listing not found.");

            var reserveResult = listing.ReserveCapacity(request.WeightKg.Value);
            if (reserveResult.IsFailure)
                return reserveResult;

            await _listingRepository.UpdateAsync(listing);
        }

        request.Accept();
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (_messageBus is not null)
        {
            await _messageBus.PublishAsync(new ServiceRequestAcceptedIntegrationEvent(
                request.Id,
                request.RequesterId,
                request.ProviderId));

            if (request.SettlementType == Domain.Enums.PaymentSettlementType.DirectHandover && request.PlatformDirectFeeAmount > 0m)
            {
                await _messageBus.PublishAsync(new MarketplacePaymentReleaseRequestedIntegrationEvent(
                    request.Id,
                    BuyerProfileId: request.RequesterId,
                    SellerProfileId: request.ProviderId,
                    CommissionAmount: request.PlatformDirectFeeAmount,
                    Amount: request.PlatformDirectFeeAmount,
                    Currency: request.AgreedCurrency ?? "EUR"));
            }
        }

        return Result.Success();
    }
}