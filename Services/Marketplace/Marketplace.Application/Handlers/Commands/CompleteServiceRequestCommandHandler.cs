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

public class CompleteServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus,
    IServiceListingRepository? _listingRepository = null,
    ICategoryRepository? _categoryRepository = null)
{
    public async Task<Result> Handle(CompleteServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.RequesterId != command.RequestedBy)
            return Result.Failure("Seul le demandeur peut confirmer la bonne exécution du service.");

        var completeResult = request.Complete();
        if (completeResult.IsFailure)
            return completeResult;

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            var amount = request.AgreedPrice ?? 0m;
            var currency = request.AgreedCurrency ?? "XOF";
            var category = request.Listing?.Category;
            if (category?.Policy is null && _listingRepository is not null && _categoryRepository is not null)
            {
                var listing = request.Listing ?? await _listingRepository.GetByIdAsync(request.ListingId);
                if (listing is not null)
                {
                    category = await _categoryRepository.GetByIdAsync(listing.CategoryId);
                }
            }
            var commissionRate = category?.Policy?.DefaultCommissionRate ?? 0.10m;
            var commissionAmount = Math.Round(amount * commissionRate, 2);

            await _messageBus.PublishAsync(
                new MarketplacePaymentReleaseRequestedIntegrationEvent(
                    request.Id,
                    BuyerProfileId: request.RequesterId,
                    SellerProfileId: request.ProviderId,
                    CommissionAmount: commissionAmount,
                    Amount: amount,
                    Currency: currency));
        }

        return Result.Success();
    }
}
