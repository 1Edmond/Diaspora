using Marketplace.Application.DTOs;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetDisputesQueryHandler(
    IDisputeRepository _disputeRepository,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<DisputeDto>>> Handle(GetDisputesQuery query, CancellationToken cancellationToken = default)
    {
        var pagedDisputes = await _disputeRepository.GetPagedDisputesAsync(
            query.Status,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var userIds = pagedDisputes.Items
            .SelectMany(d => new[] { d.InitiatorId, d.DefendantId })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var profiles = await _profileDirectory.GetByUserIdsAsync(userIds, cancellationToken);

        var items = pagedDisputes.Items.Select(d =>
        {
            profiles.TryGetValue(d.InitiatorId, out var initiator);
            profiles.TryGetValue(d.DefendantId, out var defendant);

            return new DisputeDto(
                d.Id,
                d.ServiceRequestId,
                d.InitiatorId,
                initiator?.DisplayName,
                d.DefendantId,
                defendant?.DisplayName,
                d.Reason,
                d.Description,
                d.Status,
                d.EvidenceUrls,
                d.ResolvedBy,
                d.ResolvedAt,
                d.ResolutionNotes,
                d.BuyerRefundPercentage,
                d.SellerPayoutPercentage,
                d.ServiceRequest?.AgreedPrice ?? 0m,
                d.ServiceRequest?.AgreedCurrency ?? string.Empty,
                d.ServiceRequest?.PaymentStatus ?? PaymentStatus.Pending,
                d.CreatedAt
            );
        }).ToList();

        return Result.Success(new PagedList<DisputeDto>(items, pagedDisputes.TotalCount, pagedDisputes.PageNumber, pagedDisputes.PageSize));
    }
}
