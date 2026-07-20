using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CancelServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(CancelServiceRequestCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.Id);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.RequesterId != command.RequestedBy)
            return Result.Failure("Only the requester can cancel a request.");

        request.Cancel(command.Reason);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}