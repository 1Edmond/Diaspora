using Chat.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Events;

public sealed record ConversationCreatedDomainEvent(
    Guid ConversationId, ConversationType Type, Guid CreatedByChatProfileId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}