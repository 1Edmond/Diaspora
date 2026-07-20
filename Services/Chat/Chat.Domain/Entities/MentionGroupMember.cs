using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class MentionGroupMember : Entity
{
    public Guid ChatProfileId { get; private set; }
    public Guid MentionGroupId { get; private set; }

    private MentionGroupMember() { }

    public MentionGroupMember(Guid mentionGroupId, Guid chatProfileId)
        : base(Guid.NewGuid())
    {
        MentionGroupId = mentionGroupId;
        ChatProfileId = chatProfileId;
    }
}