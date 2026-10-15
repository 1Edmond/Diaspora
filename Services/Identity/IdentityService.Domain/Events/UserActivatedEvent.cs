using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Events;

public record UserActivatedEvent(Guid UserId, string Email,
    PhoneNumber? PhoneNumber,
    string? FirstName,
    string? LastName,
    DateOnly DateOfBirth
    ) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
}