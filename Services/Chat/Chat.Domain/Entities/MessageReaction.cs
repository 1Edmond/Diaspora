using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class MessageReaction : Entity
{
    public Guid MessageId { get; private set; }
    public Guid ChatProfileId { get; private set; }
    public string Emoji { get; private set; } = string.Empty;
    public DateTime ReactedAt { get; private set; }

    private MessageReaction() { }

    public MessageReaction(Guid messageId, Guid chatProfileId, string emoji)
        : base(Guid.NewGuid())
    {
        MessageId = messageId;
        ChatProfileId = chatProfileId;
        Emoji = emoji;
        ReactedAt = DateTime.UtcNow;
    }
}