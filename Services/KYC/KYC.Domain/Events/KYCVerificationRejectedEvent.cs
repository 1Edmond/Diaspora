using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Events;

public record KYCVerificationRejectedEvent(Guid VerificationId, Guid UserId, string Reason) : IDomainEvent
{
    public Guid EventId => Guid.NewGuid();

    public DateTime OccurredOn => DateTime.UtcNow;

    public int Version => 1;
}



