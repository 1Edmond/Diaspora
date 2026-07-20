using Chat.Domain.Enums;
using Chat.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities;

public sealed class Message : AggregateRoot
{
    public Guid ConversationId { get; private set; }
    public Guid SenderChatProfileId { get; private set; }
    public MessageType Type { get; private set; }
    public string? Content { get; private set; }
    public Guid? ReplyToMessageId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? EditedAt { get; private set; }

    private readonly List<MessageAttachment> _attachments = new();
    public IReadOnlyCollection<MessageAttachment> Attachments => _attachments.AsReadOnly();

    private readonly List<MessageReaction> _reactions = new();
    public IReadOnlyCollection<MessageReaction> Reactions => _reactions.AsReadOnly();

    private readonly List<MessageMention> _mentions = new();
    public IReadOnlyCollection<MessageMention> Mentions => _mentions.AsReadOnly();

    private Message() { }

    private Message(
        Guid id, Guid conversationId, Guid senderChatProfileId,
        MessageType type, string? content, Guid? replyToMessageId)
        : base(id)
    {
        ConversationId = conversationId;
        SenderChatProfileId = senderChatProfileId;
        Type = type;
        Content = content;
        ReplyToMessageId = replyToMessageId;
        IsDeleted = false;
    }

    public static Result<Message> Create(
        Guid conversationId, Guid senderChatProfileId,
        MessageType type, string? content, Guid? replyToMessageId = null)
    {
        if (type == MessageType.Text && string.IsNullOrWhiteSpace(content))
            return Result.Failure<Message>("Text message content cannot be empty.");

        var msg = new Message(
            Guid.NewGuid(), conversationId, senderChatProfileId,
            type, content, replyToMessageId);

        msg.AddDomainEvent(new MessageSentDomainEvent(
            msg.Id, conversationId, senderChatProfileId, type, content));

        return Result.Success(msg);
    }

    // ── Attachments ───────────────────────────────────────────────────────────

    public void AddAttachment(
        AttachmentType attachmentType, string fileName, string filePath,
        string mimeType, long fileSize,
        int? durationMs = null, int? width = null, int? height = null)
        => _attachments.Add(new MessageAttachment(
            Id, attachmentType, fileName, filePath, mimeType, fileSize,
            durationMs, width, height));

    // ── Reactions ─────────────────────────────────────────────────────────────

    public Result AddReaction(Guid chatProfileId, string emoji)
    {
        if (IsDeleted) return Result.Failure("Cannot react to a deleted message.");
        if (string.IsNullOrWhiteSpace(emoji))
            return Result.Failure("Emoji is required.");

        if (_reactions.Any(r => r.ChatProfileId == chatProfileId && r.Emoji == emoji))
            return Result.Failure("Reaction already added.");

        _reactions.Add(new MessageReaction(Id, chatProfileId, emoji));
        AddDomainEvent(new ReactionAddedDomainEvent(Id, chatProfileId, emoji));
        return Result.Success();
    }

    public void RemoveReaction(Guid chatProfileId, string emoji)
        => _reactions.RemoveAll(r => r.ChatProfileId == chatProfileId && r.Emoji == emoji);

    // ── Mentions ──────────────────────────────────────────────────────────────

    public void AddMentionProfile(Guid chatProfileId)
        => _mentions.Add(MessageMention.ForProfile(Id, chatProfileId));

    public void AddMentionEveryone()
        => _mentions.Add(MessageMention.ForEveryone(Id));

    public void AddMentionGroup(Guid mentionGroupId)
        => _mentions.Add(MessageMention.ForGroup(Id, mentionGroupId));

    // ── Edit / Delete ─────────────────────────────────────────────────────────

    public Result Edit(string newContent, Guid requestingChatProfileId)
    {
        if (IsDeleted) return Result.Failure("Cannot edit a deleted message.");
        if (SenderChatProfileId != requestingChatProfileId)
            return Result.Failure("Only the sender can edit a message.");
        if (Type != MessageType.Text)
            return Result.Failure("Only text messages can be edited.");
        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure("Content cannot be empty.");

        Content = newContent;
        EditedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Delete(Guid requestingChatProfileId, bool isAdmin = false)
    {
        if (IsDeleted) return Result.Failure("Message already deleted.");

        if (!isAdmin && SenderChatProfileId != requestingChatProfileId)
            return Result.Failure("Only the sender or an admin can delete a message.");

        IsDeleted = true;
        Content = null;
        _attachments.Clear();

        AddDomainEvent(new MessageDeletedDomainEvent(Id, ConversationId));
        return Result.Success();
    }
}
