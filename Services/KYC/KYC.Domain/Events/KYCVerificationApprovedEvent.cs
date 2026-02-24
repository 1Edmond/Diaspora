using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Events;

public record KYCVerificationApprovedEvent(Guid VerificationId, Guid UserId, KYCLevel Level) : IDomainEvent
{
    public Guid EventId => Guid.NewGuid();

    public DateTime OccurredOn => DateTime.UtcNow;

    public int Version => 1;
}

