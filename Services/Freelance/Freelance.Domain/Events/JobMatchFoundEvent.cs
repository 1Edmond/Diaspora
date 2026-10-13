using Shared.Domain.Interfaces;
using System;

namespace Freelance.Domain.Events;

public record JobMatchFoundEvent(
    Guid JobPostingId,
    Guid WorkerId,
    Guid EmployerId,
    Guid CategoryId,
    string Title
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}