using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class CompleteServiceRequestCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
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

        return Result.Success();
    }
}
