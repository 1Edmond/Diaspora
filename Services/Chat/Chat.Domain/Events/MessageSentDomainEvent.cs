using Chat.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Events;

public sealed record MessageSentDomainEvent(
    Guid MessageId, Guid ConversationId, Guid SenderChatProfileId,
    MessageType Type, string? Content
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int Version => 1;
}