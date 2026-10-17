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

public class ResolveDisputeCommandHandler(
    IDisputeRepository _disputeRepository,
    IServiceRequestRepository _requestRepository,
    IMessageBus _messageBus,
    IUnitOfWork _unitOfWork,
    IServiceListingRepository? _listingRepository = null,
    ICategoryRepository? _categoryRepository = null)
{
    public async Task<Result> Handle(ResolveDisputeCommand command)
    {
        var dispute = await _disputeRepository.GetByIdAsync(command.DisputeId);
        if (dispute is null)
            return Result.Failure("Litige introuvable.");

        var request = await _requestRepository.GetByIdAsync(dispute.ServiceRequestId);
        if (request is null)
            return Result.Failure("Demande associée introuvable.");

        try
        {
            dispute.Resolve(
                command.AdminUserId,
                command.FinalStatus,
                command.ResolutionNotes,
                command.BuyerRefundPercentage,
                command.SellerPayoutPercentage);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result.Failure(ex.Message);
        }

        request.ResolveDispute();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            var totalAmount = request.AgreedPrice ?? 0m;
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

            if (command.FinalStatus == DisputeStatus.ResolvedBuyerRefunded)
            {
                await _messageBus.PublishAsync(new MarketplacePaymentRefundRequestedIntegrationEvent(
                    request.Id,
                    BuyerProfileId: request.RequesterId,
                    Amount: totalAmount,
                    Currency: currency));
            }
            else if (command.FinalStatus == DisputeStatus.ResolvedSellerPaid)
            {
                var commissionAmount = Math.Round(totalAmount * commissionRate, 2);
                await _messageBus.PublishAsync(new MarketplacePaymentReleaseRequestedIntegrationEvent(
                    request.Id,
                    BuyerProfileId: request.RequesterId,
                    SellerProfileId: request.ProviderId,
                    CommissionAmount: commissionAmount,
                    Amount: totalAmount,
                    Currency: currency));
            }
            else if (command.FinalStatus == DisputeStatus.ResolvedSplit)
            {
                var buyerAmount = Math.Round(totalAmount * (command.BuyerRefundPercentage / 100m), 2);
                var sellerAmount = totalAmount - buyerAmount;

                if (sellerAmount > 0m)
                {
                    var commissionAmount = Math.Round(sellerAmount * commissionRate, 2);
                    await _messageBus.PublishAsync(new MarketplacePaymentReleaseRequestedIntegrationEvent(
                        request.Id,
                        BuyerProfileId: request.RequesterId,
                        SellerProfileId: request.ProviderId,
                        CommissionAmount: commissionAmount,
                        Amount: sellerAmount,
                        Currency: currency));
                }

                if (buyerAmount > 0m)
                {
                    await _messageBus.PublishAsync(new MarketplacePaymentRefundRequestedIntegrationEvent(
                        request.Id,
                        BuyerProfileId: request.RequesterId,
                        Amount: buyerAmount,
                        Currency: currency));
                }
            }
        }

        await _disputeRepository.UpdateAsync(dispute);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
