using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class MarkInTransitCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(MarkInTransitCommand command, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.RequestedBy && request.RequesterId != command.RequestedBy)
            return Result.Failure("Access denied.");

        var result = request.MarkInTransit();
        if (result.IsFailure)
            return result;

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
