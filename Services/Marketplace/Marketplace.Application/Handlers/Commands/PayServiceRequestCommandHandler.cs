using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public sealed class PayServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IServiceListingRepository _listingRepository,
    IProfileDirectoryRepository _profileDirectoryRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus,
    ILogger<PayServiceRequestCommandHandler> _logger)
{
    public async Task<Result> Handle(
        PayServiceRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.RequesterId != command.CurrentUserId)
        {
            _logger.LogWarning(
                "[MarketplaceService] User {UserId} attempted to pay for ServiceRequest {RequestId} owned by {RequesterId}",
                command.CurrentUserId, command.Id, request.RequesterId);
            return Result.Failure("Access denied.");
        }

        if (request.Status != ServiceRequestStatus.Accepted)
            return Result.Failure("Service request must be accepted to proceed with payment.");

        var listing = await _listingRepository.GetByIdAsync(request.ListingId);
        if (listing is null)
            return Result.Failure("Associated listing not found.");

        if (listing.PaymentMode == PaymentMode.ContactOnly)
            return Result.Failure("This listing does not support platform payment.");

        if (!request.AgreedPrice.HasValue || request.AgreedPrice.Value <= 0)
            return Result.Failure("Agreed price must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.AgreedCurrency))
            return Result.Failure("Agreed currency is required.");

        if (request.PaymentStatus != PaymentStatus.Pending && request.PaymentStatus != PaymentStatus.Failed)
            return Result.Failure($"Payment cannot be initiated when payment status is {request.PaymentStatus}.");

        var buyerProfile = await _profileDirectoryRepository.GetByUserIdAsync(
            request.RequesterId, cancellationToken);
        if (buyerProfile is null)
            return Result.Failure("Buyer profile directory entry not found.");

        var sellerProfile = await _profileDirectoryRepository.GetByUserIdAsync(
            request.ProviderId, cancellationToken);
        if (sellerProfile is null)
            return Result.Failure("Seller profile directory entry not found.");

        request.MarkPaymentProcessing();
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[MarketplaceService] ServiceRequest {RequestId} moved to Processing. Publishing MarketplacePaymentRequested (Amount: {Amount} {Currency})",
            request.Id, request.AgreedPrice.Value, request.AgreedCurrency);

        await _messageBus.PublishAsync(new MarketplacePaymentRequestedIntegrationEvent(
            request.Id,
            buyerProfile.Id,
            sellerProfile.Id,
            request.AgreedPrice.Value,
            request.AgreedCurrency!
        ));

        return Result.Success();
    }
}
