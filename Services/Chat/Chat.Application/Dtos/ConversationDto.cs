using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record ConversationDto(
    Guid Id,
    ConversationType Type,
    string? Title,
    string? Description,
    string? AvatarUrl,
    int ParticipantCount,
    int UnreadCount,
    DateTime CreatedAt
);