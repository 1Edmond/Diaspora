using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Events;

public record KYCDocumentRemovedEvent(Guid VerificationId, Guid UserId, Guid DocumentId) : IDomainEvent
{
    public Guid EventId => Guid.NewGuid();

    public DateTime OccurredOn => DateTime.UtcNow;

    public int Version => 1;
}
