using Shared.Domain.Interfaces;
using System;

namespace Freelance.Domain.Events;

public record JobApplicationRejectedEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid DecidedBy,
    string? Reason
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}