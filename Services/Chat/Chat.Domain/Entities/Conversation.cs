using Chat.Domain.Enums;
using Chat.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class Conversation : AggregateRoot
{
    public ConversationType Type { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string? AvatarUrl { get; private set; }
    public Guid CreatedByChatProfileId { get; private set; }
    public Guid ProfileTypeId { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ConversationParticipant> _participants = new();
    public IReadOnlyCollection<ConversationParticipant> Participants
        => _participants.AsReadOnly();

    private readonly List<MentionGroup> _mentionGroups = new();
    public IReadOnlyCollection<MentionGroup> MentionGroups
        => _mentionGroups.AsReadOnly();

    private Conversation() { }

    private Conversation(
        Guid id, ConversationType type, string? title,
        string? description, string? avatarUrl, Guid createdByChatProfileId)
        : base(id)
    {
        Type = type;
        Title = title;
        Description = description;
        AvatarUrl = avatarUrl;
        CreatedByChatProfileId = createdByChatProfileId;
        IsActive = true;
    }

    // ── Factory methods ───────────────────────────────────────────────────────

    public static Result<Conversation> CreateDirect(
        Guid initiatorChatProfileId, Guid recipientChatProfileId)
    {
        if (initiatorChatProfileId == recipientChatProfileId)
            return Result.Failure<Conversation>("Cannot create a DM with yourself.");

        var conv = new Conversation(
            Guid.NewGuid(), ConversationType.DirectMessage,
            null, null, null, initiatorChatProfileId);

        conv._participants.Add(new ConversationParticipant(
            conv.Id, initiatorChatProfileId, ParticipantRole.Owner));
        conv._participants.Add(new ConversationParticipant(
            conv.Id, recipientChatProfileId, ParticipantRole.Member));

        conv.AddDomainEvent(new ConversationCreatedDomainEvent(
            conv.Id, ConversationType.DirectMessage, initiatorChatProfileId));

        return Result.Success(conv);
    }

    public static Result<Conversation> CreateGroup(
        string title, string? description, string? avatarUrl,
        Guid createdByChatProfileId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Conversation>("Group title is required.");

        var conv = new Conversation(
            Guid.NewGuid(), ConversationType.Group,
            title, description, avatarUrl, createdByChatProfileId);

        conv._participants.Add(new ConversationParticipant(
            conv.Id, createdByChatProfileId, ParticipantRole.Owner));

        conv.AddDomainEvent(new ConversationCreatedDomainEvent(
            conv.Id, ConversationType.Group, createdByChatProfileId));

        return Result.Success(conv);
    }

    public static Result<Conversation> CreateCommunity(
        string title, string? description, string? avatarUrl,
        Guid createdByChatProfileId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Conversation>("Community title is required.");

        var conv = new Conversation(
            Guid.NewGuid(), ConversationType.Community,
            title, description, avatarUrl, createdByChatProfileId);

        conv._participants.Add(new ConversationParticipant(
            conv.Id, createdByChatProfileId, ParticipantRole.Owner));

        conv.AddDomainEvent(new ConversationCreatedDomainEvent(
            conv.Id, ConversationType.Community, createdByChatProfileId));

        return Result.Success(conv);
    }

    public static Result<Conversation> CreateBroadcast(
        string title, string? description, Guid createdByChatProfileId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Conversation>("Broadcast title is required.");

        var conv = new Conversation(
            Guid.NewGuid(), ConversationType.Broadcast,
            title, description, null, createdByChatProfileId);

        conv._participants.Add(new ConversationParticipant(
            conv.Id, createdByChatProfileId, ParticipantRole.Owner));

        conv.AddDomainEvent(new ConversationCreatedDomainEvent(
            conv.Id, ConversationType.Broadcast, createdByChatProfileId));

        return Result.Success(conv);
    }

    // ── Participants ──────────────────────────────────────────────────────────

    public Result AddParticipant(Guid chatProfileId, ParticipantRole role = ParticipantRole.Member)
    {
        if (Type == ConversationType.DirectMessage && _participants.Count >= 2)
            return Result.Failure("Direct messages can only have 2 participants.");

        var existing = _participants.FirstOrDefault(p => p.ChatProfileId == chatProfileId);
        if (existing is not null)
        {
            if (existing.IsActive)
                return Result.Failure("Participant already in conversation.");
            existing.Rejoin();
            return Result.Success();
        }

        _participants.Add(new ConversationParticipant(Id, chatProfileId, role));
        AddDomainEvent(new ParticipantAddedDomainEvent(Id, chatProfileId));
        return Result.Success();
    }

    public Result RemoveParticipant(Guid chatProfileId)
    {
        if (Type == ConversationType.DirectMessage)
            return Result.Failure("Cannot remove participants from a direct message.");

        var participant = _participants.FirstOrDefault(
            p => p.ChatProfileId == chatProfileId && p.IsActive);

        if (participant is null)
            return Result.Failure("Participant not found.");

        if (participant.Role == ParticipantRole.Owner)
            return Result.Failure("Cannot remove the owner of a conversation.");

        participant.Remove();
        AddDomainEvent(new ParticipantRemovedDomainEvent(Id, chatProfileId));
        return Result.Success();
    }

    public Result SetParticipantRole(Guid chatProfileId, ParticipantRole role)
    {
        var participant = _participants.FirstOrDefault(
            p => p.ChatProfileId == chatProfileId && p.IsActive);

        if (participant is null)
            return Result.Failure("Participant not found.");

        participant.SetRole(role);
        return Result.Success();
    }

    public Result MarkAsRead(Guid chatProfileId)
    {
        var participant = _participants.FirstOrDefault(
            p => p.ChatProfileId == chatProfileId && p.IsActive);

        if (participant is null)
            return Result.Failure("Participant not found.");

        participant.MarkAsRead();
        return Result.Success();
    }

    // ── Mention Groups ────────────────────────────────────────────────────────

    public Result<MentionGroup> AddMentionGroup(string name)
    {
        if (_mentionGroups.Any(g => g.Name == name.ToLowerInvariant().TrimStart('@')))
            return Result.Failure<MentionGroup>($"Mention group '@{name}' already exists.");

        var group = new MentionGroup(Id, name);
        _mentionGroups.Add(group);
        return Result.Success(group);
    }

    // ── Metadata ──────────────────────────────────────────────────────────────

    public void UpdateMetadata(string? title, string? description, string? avatarUrl)
    {
        if (Type != ConversationType.DirectMessage)
            Title = title;
        Description = description;
        AvatarUrl = avatarUrl;
    }

    public void Archive() => IsActive = false;
    public void Restore() => IsActive = true;

    public bool HasParticipant(Guid chatProfileId)
        => _participants.Any(p => p.ChatProfileId == chatProfileId && p.IsActive);

    public bool IsAdmin(Guid chatProfileId)
        => _participants.Any(p => p.ChatProfileId == chatProfileId && p.IsActive
            && p.Role is ParticipantRole.Admin or ParticipantRole.Owner);
}
