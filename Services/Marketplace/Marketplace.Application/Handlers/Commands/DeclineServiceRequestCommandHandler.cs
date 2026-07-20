using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class DeclineServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
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

        return Result.Success();
    }
}
