using Chat.Domain.Enums;
using Chat.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class Call : AggregateRoot
{
    public Guid ConversationId { get; private set; }
    public Guid InitiatorChatProfileId { get; private set; }
    public CallType Type { get; private set; }
    public CallStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue
                                        ? EndedAt - StartedAt : null;

    private readonly List<CallParticipant> _participants = new();
    public IReadOnlyCollection<CallParticipant> Participants => _participants.AsReadOnly();

    private Call() { }

    private Call(
        Guid id, Guid conversationId,
        Guid initiatorChatProfileId, CallType type)
        : base(id)
    {
        ConversationId = conversationId;
        InitiatorChatProfileId = initiatorChatProfileId;
        Type = type;
        Status = CallStatus.Ringing;
    }

    public static Result<Call> Create(
        Guid conversationId, Guid initiatorChatProfileId, CallType type)
    {
        var call = new Call(
            Guid.NewGuid(), conversationId, initiatorChatProfileId, type);

        call._participants.Add(new CallParticipant(call.Id, initiatorChatProfileId));
        call._participants.First().Join(); // l'initiateur rejoint immédiatement

        call.StartedAt = DateTime.UtcNow;

        call.AddDomainEvent(new CallStartedDomainEvent(
            call.Id, conversationId, initiatorChatProfileId, type));

        return Result.Success(call);
    }

    public Result AddParticipant(Guid chatProfileId)
    {
        if (Status is CallStatus.Ended or CallStatus.Missed or CallStatus.Rejected)
            return Result.Failure("Cannot join a call that has ended.");

        if (_participants.Any(p => p.ChatProfileId == chatProfileId))
            return Result.Failure("Participant already in this call.");

        _participants.Add(new CallParticipant(Id, chatProfileId));
        Status = CallStatus.Active;
        return Result.Success();
    }

    public Result JoinParticipant(Guid chatProfileId)
    {
        var participant = _participants.FirstOrDefault(p => p.ChatProfileId == chatProfileId);
        if (participant is null)
            return Result.Failure("Participant not invited to this call.");

        participant.Join();
        Status = CallStatus.Active;
        return Result.Success();
    }

    public Result LeaveParticipant(Guid chatProfileId)
    {
        var participant = _participants.FirstOrDefault(
            p => p.ChatProfileId == chatProfileId);

        if (participant is null)
            return Result.Failure("Participant not found.");

        participant.Leave();

        var activeCount = _participants.Count(p =>
            p.Status == CallParticipantStatus.Joined);

        if (activeCount == 0) End(CallStatus.Ended);
        return Result.Success();
    }

    public Result DeclineParticipant(Guid chatProfileId)
    {
        var participant = _participants.FirstOrDefault(p => p.ChatProfileId == chatProfileId);
        if (participant is null)
            return Result.Failure("Participant not found.");

        participant.Decline();

        var allDeclinedOrLeft = _participants.All(p =>
            p.Status is CallParticipantStatus.Declined or CallParticipantStatus.Left);

        if (allDeclinedOrLeft) End(CallStatus.Rejected);
        return Result.Success();
    }

    public void End(CallStatus finalStatus = CallStatus.Ended)
    {
        Status = finalStatus;
        EndedAt = DateTime.UtcNow;
        AddDomainEvent(new CallEndedDomainEvent(Id, ConversationId, finalStatus, Duration));
    }

    public void MarkMissed()
    {
        if (Status == CallStatus.Ringing)
            End(CallStatus.Missed);
    }
}
