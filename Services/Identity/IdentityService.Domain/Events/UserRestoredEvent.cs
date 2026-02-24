using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Events;

public record UserRestoredEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRestoredEvent() : base() { }
    public UserRestoredEvent(string userId, string email) : base()
    {
        UserId = userId;
        Email = email;
    }
}
