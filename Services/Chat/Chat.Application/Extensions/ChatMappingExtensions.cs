using Chat.Application.Dtos;
using Chat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Extensions;

public static class ChatMappingExtensions
{
    public static ChatProfileDto ToDto(this ChatProfile p) =>
        new(p.Id, p.UserId, p.ProfileId, p.DisplayName,
            p.AvatarUrl, p.IsBot, p.Status, p.LastSeenAt, p.IsActive, p.CreatedAt);

    public static ConversationDto ToDto(this Conversation c, int unreadCount) =>
        new(c.Id, c.Type, c.Title, c.Description, c.AvatarUrl,
            c.Participants.Count(p => p.IsActive), unreadCount, c.CreatedAt);

    public static MessageDto ToDto(
        this Message m,
        string senderDisplayName,
        string? senderAvatarUrl,
        Guid requestingChatProfileId) =>
        new(
            m.Id, m.ConversationId, m.SenderChatProfileId,
            senderDisplayName, senderAvatarUrl,
            m.Type, m.Content, m.ReplyToMessageId,
            m.IsDeleted, m.EditedAt,
            m.Attachments.Select(a => new AttachmentDto(
                a.Id, a.Type, a.FileName, a.MimeType,
                a.FileSize, a.DurationMs, a.Width, a.Height)).ToList(),
            m.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto(
                    g.Key, g.Count(),
                    g.Any(r => r.ChatProfileId == requestingChatProfileId)))
                .ToList(),
            m.Mentions.Select(mn => new MentionDto(
                mn.Type, mn.ChatProfileId, mn.MentionGroupId)).ToList(),
            m.CreatedAt);
}
