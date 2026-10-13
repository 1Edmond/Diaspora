using Marketplace.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Entities;

/// <summary>Signalement d'un listing, d'un avis ou d'un utilisateur pour comportement abusif.</summary>
public class Report : AggregateRoot
{
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public Guid ReporterId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; }

    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }

    private Report() { }

    public static Report Create(ReportTargetType targetType, Guid targetId, Guid reporterId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty.");

        return new Report
        {
            Id = Guid.NewGuid(),
            TargetType = targetType,
            TargetId = targetId,
            ReporterId = reporterId,
            Reason = reason,
            Status = ReportStatus.Pending
        };
    }

    public void Resolve(Guid reviewedBy, ReportStatus newStatus, string? resolutionNotes)
    {
        if (newStatus == ReportStatus.Pending)
            throw new ArgumentException("Cannot resolve a report back to Pending.");

        Status = newStatus;
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        ResolutionNotes = resolutionNotes;
    }
}