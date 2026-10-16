using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record ProfileVerifiedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    string VerifiedByAdminId,
    DateTime VerifiedAt,
    Shared.Domain.Enums.VerificationLevel Level = Shared.Domain.Enums.VerificationLevel.L1_IdentityDeclared
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}