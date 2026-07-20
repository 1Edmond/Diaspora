using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record ExternalProfileActivatedDomainEvent(
    Guid ProfileId,
    Guid ProfileTypeId,
    Guid UserId,
    string CountryCode,
    string Currency,
    DateTime ActivatedAt
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}