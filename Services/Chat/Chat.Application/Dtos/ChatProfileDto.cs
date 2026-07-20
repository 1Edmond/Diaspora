using Chat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos;

public sealed record ChatProfileDto(
    Guid Id,
    Guid UserId,
    Guid? ExternalProfileId,
    string DisplayName,
    string? AvatarUrl,
    bool IsBot,
    ChatProfileStatus Status,
    DateTime? LastSeenAt,
    bool IsActive,
    DateTime CreatedAt
);
