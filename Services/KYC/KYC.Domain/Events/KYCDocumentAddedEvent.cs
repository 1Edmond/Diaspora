using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Events;

public record KYCDocumentAddedEvent(Guid VerificationId, Guid UserId, Guid DocumentId, string DocumentType) : IDomainEvent
{
    public Guid EventId => Guid.NewGuid();

    public DateTime OccurredOn => DateTime.UtcNow;

    public int Version => 1;
}

