using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Domain.Events;

public record ReviewCreatedEvent(
    Guid ReviewId,
    Guid ListingId,
    Guid ReviewerId,
    int Rating
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}