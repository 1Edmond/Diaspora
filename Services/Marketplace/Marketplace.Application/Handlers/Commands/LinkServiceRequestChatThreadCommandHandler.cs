using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class LinkServiceRequestChatThreadCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(LinkServiceRequestChatThreadCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.ServiceRequestId);
        if (request is null)
            return Result.Failure("Service request not found.");

        // Seuls le demandeur ou le prestataire concernés peuvent lier un fil de discussion
        if (request.RequesterId != command.RequestedBy && request.ProviderId != command.RequestedBy)
            return Result.Failure("You are not a participant of this service request.");

        request.LinkChatThread(command.ChatThreadId);

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}