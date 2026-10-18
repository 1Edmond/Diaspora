using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class ConfirmHandoverCommandHandler(
    IServiceRequestRepository _requestRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(ConfirmHandoverCommand command, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure("Request not found.");

        if (request.ProviderId != command.RequestedBy)
            return Result.Failure("Seul le transporteur prestataire peut confirmer la prise en charge du colis.");

        var result = request.ConfirmHandover(command.Otp, command.ProofPhotoUrl);
        if (result.IsFailure)
        {
            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
