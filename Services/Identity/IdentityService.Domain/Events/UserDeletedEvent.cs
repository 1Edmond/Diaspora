using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Domain.Events;

public record UserDeletedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int Version => 1;
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DeleteBy { get; init; } = string.Empty;

    public UserDeletedEvent() : base() { }

    public UserDeletedEvent(string userId, string email, string DeleteBy) : base()
    {
        UserId = userId;
        Email = email;
    }
}