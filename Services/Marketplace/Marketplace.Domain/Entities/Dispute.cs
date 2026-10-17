using Marketplace.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;

namespace Marketplace.Domain.Entities;

public class Dispute : AggregateRoot
{
    public Guid ServiceRequestId { get; private set; }
    public Guid InitiatorId { get; private set; }
    public Guid DefendantId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DisputeStatus Status { get; private set; }

    private readonly List<string> _evidenceUrls = new();
    public IReadOnlyCollection<string> EvidenceUrls => _evidenceUrls.AsReadOnly();

    public Guid? ResolvedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public decimal? BuyerRefundPercentage { get; private set; }
    public decimal? SellerPayoutPercentage { get; private set; }

    public ServiceRequest ServiceRequest { get; private set; } = null!;

    private Dispute() { }

    public static Dispute Create(
        Guid serviceRequestId,
        Guid initiatorId,
        Guid defendantId,
        string reason,
        string description,
        IEnumerable<string>? evidenceUrls = null)
    {
        if (serviceRequestId == Guid.Empty)
            throw new ArgumentException("ServiceRequestId is required.", nameof(serviceRequestId));
        if (initiatorId == Guid.Empty)
            throw new ArgumentException("InitiatorId is required.", nameof(initiatorId));
        if (defendantId == Guid.Empty)
            throw new ArgumentException("DefendantId is required.", nameof(defendantId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            ServiceRequestId = serviceRequestId,
            InitiatorId = initiatorId,
            DefendantId = defendantId,
            Reason = reason.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Status = DisputeStatus.Opened
        };

        if (evidenceUrls != null)
        {
            dispute._evidenceUrls.AddRange(evidenceUrls);
        }

        return dispute;
    }

    public void AddEvidence(string url)
    {
        if (!string.IsNullOrWhiteSpace(url) && !_evidenceUrls.Contains(url))
            _evidenceUrls.Add(url.Trim());
    }

    public void MarkUnderReview()
    {
        if (Status == DisputeStatus.Opened)
            Status = DisputeStatus.UnderReview;
    }

    public void Resolve(
        Guid adminUserId,
        DisputeStatus finalStatus,
        string resolutionNotes,
        decimal buyerRefundPercentage,
        decimal sellerPayoutPercentage)
    {
        if (Status is DisputeStatus.ResolvedBuyerRefunded or DisputeStatus.ResolvedSellerPaid or DisputeStatus.ResolvedSplit or DisputeStatus.Dismissed)
            throw new InvalidOperationException("Ce litige est déjà résolu ou clôturé.");

        if (finalStatus == DisputeStatus.Opened || finalStatus == DisputeStatus.UnderReview)
            throw new ArgumentException("Resolution status must be a terminal resolved status.", nameof(finalStatus));

        if (buyerRefundPercentage < 0 || buyerRefundPercentage > 100 ||
            sellerPayoutPercentage < 0 || sellerPayoutPercentage > 100)
            throw new ArgumentOutOfRangeException("Percentages must be between 0 and 100.");

        if (finalStatus == DisputeStatus.ResolvedSplit && (buyerRefundPercentage + sellerPayoutPercentage != 100m))
            throw new ArgumentException("For a split resolution, buyer refund percentage and seller payout percentage must sum to 100.");

        ResolvedBy = adminUserId;
        ResolvedAt = DateTime.UtcNow;
        Status = finalStatus;
        ResolutionNotes = resolutionNotes;
        BuyerRefundPercentage = buyerRefundPercentage;
        SellerPayoutPercentage = sellerPayoutPercentage;
    }
}
