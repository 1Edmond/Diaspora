using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class MentionGroup : Entity
{
    public Guid ConversationId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private readonly List<MentionGroupMember> _members = new();
    public IReadOnlyCollection<MentionGroupMember> Members => _members.AsReadOnly();

    private MentionGroup() { }

    public MentionGroup(Guid conversationId, string name) : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        Name = name.ToLowerInvariant().TrimStart('@');
    }

    public void AddMember(Guid chatProfileId)
    {
        if (!_members.Any(m => m.ChatProfileId == chatProfileId))
            _members.Add(new MentionGroupMember(Id, chatProfileId));
    }

    public void RemoveMember(Guid chatProfileId)
        => _members.RemoveAll(m => m.ChatProfileId == chatProfileId);

    public void Rename(string name) => Name = name.ToLowerInvariant().TrimStart('@');
}

