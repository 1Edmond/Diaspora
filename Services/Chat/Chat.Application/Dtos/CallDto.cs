using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record CallDto(
    Guid Id,
    Guid ConversationId,
    Guid InitiatorChatProfileId,
    CallType Type,
    CallStatus Status,
    DateTime? StartedAt,
    DateTime? EndedAt,
    TimeSpan? Duration,
    int ParticipantCount,
    DateTime CreatedAt
);