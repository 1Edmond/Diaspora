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
    IMessageBus _messageBus)
{
    public async Task<Result> Handle(CompleteServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.RequestedBy && request.RequesterId != command.RequestedBy)
            return Result.Failure("Access denied.");

        request.Complete();
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            await _messageBus.PublishAsync(
                new MarketplacePaymentReleaseRequestedIntegrationEvent(request.Id));
        }

        return Result.Success();
    }
}
