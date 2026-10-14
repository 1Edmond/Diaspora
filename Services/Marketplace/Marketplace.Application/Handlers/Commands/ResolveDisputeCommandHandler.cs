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

public class ResolveDisputeCommandHandler(
    IDisputeRepository _disputeRepository,
    IServiceRequestRepository _requestRepository,
    IMessageBus _messageBus,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(ResolveDisputeCommand command)
    {
        var dispute = await _disputeRepository.GetByIdAsync(command.DisputeId);
        if (dispute is null)
            return Result.Failure("Litige introuvable.");

        var request = await _requestRepository.GetByIdAsync(dispute.ServiceRequestId);
        if (request is null)
            return Result.Failure("Demande associée introuvable.");

        dispute.Resolve(
            command.AdminUserId,
            command.FinalStatus,
            command.ResolutionNotes,
            command.BuyerRefundPercentage,
            command.SellerPayoutPercentage);

        request.ResolveDispute();

        if (command.FinalStatus == DisputeStatus.ResolvedBuyerRefunded)
        {
            if (request.PaymentStatus == PaymentStatus.Held)
            {
                await _messageBus.PublishAsync(new MarketplacePaymentRefundRequestedIntegrationEvent(request.Id));
            }
        }
        else if (command.FinalStatus == DisputeStatus.ResolvedSellerPaid)
        {
            if (request.PaymentStatus == PaymentStatus.Held)
            {
                await _messageBus.PublishAsync(new MarketplacePaymentReleaseRequestedIntegrationEvent(request.Id));
            }
        }

        await _disputeRepository.UpdateAsync(dispute);
        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
