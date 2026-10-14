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

public class DeclineServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork,
    IMessageBus _messageBus)
{
    public async Task<Result> Handle(DeclineServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.ProviderId)
            return Result.Failure("You are not the provider for this request.");

        request.Decline(command.Reason);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        if (request.PaymentStatus == PaymentStatus.Held)
        {
            await _messageBus.PublishAsync(
                new MarketplacePaymentRefundRequestedIntegrationEvent(request.Id));
        }

        return Result.Success();
    }
}
