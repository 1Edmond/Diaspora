using Shared.Domain.Interfaces;
using System;

namespace Freelance.Domain.Events;

public record JobApplicationAcceptedEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid DecidedBy
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}