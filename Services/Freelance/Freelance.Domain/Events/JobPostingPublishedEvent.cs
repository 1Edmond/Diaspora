using Shared.Domain.Interfaces;
using System;

namespace Freelance.Domain.Events;

public record JobPostingPublishedEvent(
    Guid JobPostingId,
    Guid EmployerId,
    Guid CategoryId,
    string Title,
    string? City,
    string? Country,
    string[] RequiredSkills
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}