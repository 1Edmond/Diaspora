using Profile.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Events;

public sealed record ExternalProfileCreatedDomainEvent(
    Guid ProfileId,
    Guid InternalProfileId,
    Guid UserId,
    Guid ExternalProfileTypeId,
    ExternalProfileKind Kind
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}