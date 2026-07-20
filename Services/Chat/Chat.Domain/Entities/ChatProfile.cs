using Chat.Domain.Enums;
using Chat.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class ChatProfile : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid? ExternalProfileId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public bool IsBot { get; private set; }
    public string? BotToken { get; private set; }  // chiffré
    public ChatProfileStatus Status { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public bool IsActive { get; private set; }

    private ChatProfile() { }

    private ChatProfile(
        Guid id, Guid userId, Guid? externalProfileId,
        string displayName, string? avatarUrl, bool isBot, string? botToken)
        : base(id)
    {
        UserId = userId;
        ExternalProfileId = externalProfileId;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        IsBot = isBot;
        BotToken = botToken;
        Status = ChatProfileStatus.Offline;
        IsActive = true;
    }

    public static Result<ChatProfile> Create(
        Guid userId, Guid? externalProfileId,
        string displayName, string? avatarUrl = null,
        bool isBot = false, string? botToken = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<ChatProfile>("UserId is required.");
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<ChatProfile>("DisplayName is required.");
        if (isBot && string.IsNullOrWhiteSpace(botToken))
            return Result.Failure<ChatProfile>("BotToken is required for bots.");

        var profile = new ChatProfile(
            Guid.NewGuid(), userId, externalProfileId,
            displayName, avatarUrl, isBot, botToken);

        profile.AddDomainEvent(new ChatProfileCreatedDomainEvent(
            profile.Id, userId, externalProfileId, isBot));

        return Result.Success(profile);
    }

    public void UpdateDisplayName(string displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName;
    }

    public void UpdateAvatar(string? avatarUrl) => AvatarUrl = avatarUrl;

    public void SetStatus(ChatProfileStatus status)
    {
        Status = status;
        if (status == ChatProfileStatus.Offline)
            LastSeenAt = DateTime.UtcNow;

        AddDomainEvent(new ChatProfileStatusChangedDomainEvent(Id, status));
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = ChatProfileStatus.Offline;
        LastSeenAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
}
