using Chat.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class CallParticipant : Entity
{
    public Guid CallId { get; private set; }
    public Guid ChatProfileId { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public CallParticipantStatus Status { get; private set; }

    // Navigation
    public ChatProfile ChatProfile { get; private set; } = null!;

    private CallParticipant() { }

    public CallParticipant(Guid callId, Guid chatProfileId) : base(Guid.NewGuid())
    {
        CallId = callId;
        ChatProfileId = chatProfileId;
        InvitedAt = DateTime.UtcNow;
        Status = CallParticipantStatus.Invited;
    }

    public void Join() { Status = CallParticipantStatus.Joined; JoinedAt = DateTime.UtcNow; }
    public void Leave() { Status = CallParticipantStatus.Left; LeftAt = DateTime.UtcNow; }
    public void Decline() { Status = CallParticipantStatus.Declined; LeftAt = DateTime.UtcNow; }
}