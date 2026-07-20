using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Commands;

public class MarkRequestPaymentCompletedCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(MarkRequestPaymentCompletedCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId);
        if (request is null)
            return Result.Failure("Request not found.");

        request.MarkPaymentCompleted(command.TransactionId);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}