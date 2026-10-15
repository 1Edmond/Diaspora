using Marketplace.Application.DTOs;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Shared.Domain.Commons;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetDisputeByIdQueryHandler(
    IDisputeRepository _disputeRepository,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<DisputeDto>> Handle(GetDisputeByIdQuery query, CancellationToken cancellationToken = default)
    {
        var dispute = await _disputeRepository.GetByIdWithDetailsAsync(query.DisputeId, cancellationToken);
        if (dispute is null)
            return Result.Failure<DisputeDto>("Litige introuvable.");

        var userIds = new[] { dispute.InitiatorId, dispute.DefendantId }
            .Where(id => id != Guid.Empty)
            .Distinct();

        var profiles = await _profileDirectory.GetByUserIdsAsync(userIds, cancellationToken);
        profiles.TryGetValue(dispute.InitiatorId, out var initiator);
        profiles.TryGetValue(dispute.DefendantId, out var defendant);

        var dto = new DisputeDto(
            dispute.Id,
            dispute.ServiceRequestId,
            dispute.InitiatorId,
            initiator?.DisplayName,
            dispute.DefendantId,
            defendant?.DisplayName,
            dispute.Reason,
            dispute.Description,
            dispute.Status,
            dispute.EvidenceUrls,
            dispute.ResolvedBy,
            dispute.ResolvedAt,
            dispute.ResolutionNotes,
            dispute.BuyerRefundPercentage,
            dispute.SellerPayoutPercentage,
            dispute.ServiceRequest?.AgreedPrice ?? 0m,
            dispute.ServiceRequest?.AgreedCurrency ?? string.Empty,
            dispute.ServiceRequest?.PaymentStatus ?? PaymentStatus.Pending,
            dispute.CreatedAt
        );

        return Result.Success(dto);
    }
}
