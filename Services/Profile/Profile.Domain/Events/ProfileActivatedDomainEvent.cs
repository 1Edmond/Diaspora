using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record ProfileActivatedDomainEvent(
    Guid ProfileId,
    Guid ProfileTypeId,
    Guid UserId,
    string CountryCode,
    string Currency,
    DateTime ActivatedAt,
    Shared.Domain.Enums.VerificationLevel Level = Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}