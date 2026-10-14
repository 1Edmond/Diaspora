using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class OpenDisputeCommandHandler(
    IServiceRequestRepository _requestRepository,
    IDisputeRepository _disputeRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(OpenDisputeCommand command)
    {
        var request = await _requestRepository.GetByIdAsync(command.ServiceRequestId);
        if (request is null)
            return Result.Failure<Guid>("Demande introuvable.");

        if (command.InitiatorUserId != request.RequesterId && command.InitiatorUserId != request.ProviderId)
            return Result.Failure<Guid>("Seules les parties prenantes de la demande peuvent ouvrir un litige.");

        var defendantId = command.InitiatorUserId == request.RequesterId
            ? request.ProviderId
            : request.RequesterId;

        var dispute = Dispute.Create(
            request.Id,
            command.InitiatorUserId,
            defendantId,
            command.Reason,
            command.Description,
            command.EvidenceUrls);

        var openResult = request.OpenDispute(dispute.Id);
        if (openResult.IsFailure)
            return Result.Failure<Guid>(openResult.Error);

        await _disputeRepository.AddAsync(dispute);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(dispute.Id);
    }
}
