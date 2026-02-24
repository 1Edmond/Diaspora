using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Domain.Events;

public sealed record DocumentUploadedDomainEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string FileName
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}