using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;

namespace Marketplace.Application.Handlers.Commands;

public class ConfirmDeliveryCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus)
{
    public async Task<Result> Handle(ConfirmDeliveryCommand command, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.RequestedBy && request.RequesterId != command.RequestedBy)
            return Result.Failure("Access denied.");

        var result = request.ConfirmDelivery(command.Otp, command.ProofPhotoUrl);
        if (result.IsFailure)
        {
            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            await _messageBus.PublishAsync(
                new MarketplacePaymentReleaseRequestedIntegrationEvent(request.Id));
        }

        return Result.Success();
    }
}
