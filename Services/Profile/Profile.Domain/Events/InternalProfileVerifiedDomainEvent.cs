using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record InternalProfileVerifiedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    string VerifiedByAdminId,
    DateTime VerifiedAt
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}