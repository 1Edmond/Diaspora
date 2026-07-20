using Chat.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class ConversationParticipant : Entity
{
    public Guid ConversationId { get; private set; }
    public Guid ChatProfileId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastReadAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public ChatProfile ChatProfile { get; private set; } = null!;

    private ConversationParticipant() { }

    public ConversationParticipant(
        Guid conversationId, Guid chatProfileId,
        ParticipantRole role = ParticipantRole.Member)
        : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        ChatProfileId = chatProfileId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void MarkAsRead() => LastReadAt = DateTime.UtcNow;
    public void SetRole(ParticipantRole role) => Role = role;
    public void Remove() => IsActive = false;
    public void Rejoin()
    {
        IsActive = true;
        JoinedAt = DateTime.UtcNow;
    }
}
