using Profile.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record ProfileKindChangedDomainEvent(
    Guid ProfileId,
    Guid UserId,
    ProfileKind OldKind,
    ProfileKind NewKind
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}
