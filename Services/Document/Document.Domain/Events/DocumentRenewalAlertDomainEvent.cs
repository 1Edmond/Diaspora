using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Domain.Events;

public sealed record DocumentRenewalAlertDomainEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    string DocumentTypeName,
    DateTime ExpiresAt,
    int DaysRemaining
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}