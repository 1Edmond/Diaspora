using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderChatProfileId,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    MessageType Type,
    string? Content,
    Guid? ReplyToMessageId,
    bool IsDeleted,
    DateTime? EditedAt,
    IReadOnlyList<AttachmentDto> Attachments,
    IReadOnlyList<ReactionDto> Reactions,
    IReadOnlyList<MentionDto> Mentions,
    DateTime CreatedAt
);