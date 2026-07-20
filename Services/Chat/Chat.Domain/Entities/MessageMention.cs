using Chat.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class MessageMention : Entity
{
    public Guid MessageId { get; private set; }
    public MentionType Type { get; private set; }
    public Guid? ChatProfileId { get; private set; }   // si Profile
    public Guid? MentionGroupId { get; private set; }   // si Group

    private MessageMention() { }

    private MessageMention(
        Guid id, Guid messageId, MentionType type,
        Guid? chatProfileId, Guid? mentionGroupId)
        : base(id)
    {
        MessageId = messageId;
        Type = type;
        ChatProfileId = chatProfileId;
        MentionGroupId = mentionGroupId;
    }

    public static MessageMention ForProfile(Guid messageId, Guid chatProfileId)
        => new(Guid.NewGuid(), messageId, MentionType.Profile, chatProfileId, null);

    public static MessageMention ForEveryone(Guid messageId)
        => new(Guid.NewGuid(), messageId, MentionType.Everyone, null, null);

    public static MessageMention ForGroup(Guid messageId, Guid mentionGroupId)
        => new(Guid.NewGuid(), messageId, MentionType.Group, null, mentionGroupId);
}

