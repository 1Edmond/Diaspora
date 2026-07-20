using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Requests;

public sealed record CreateChatProfileRequest(
    Guid UserId,
    Guid? ExternalProfileId,
    string DisplayName,
    string? AvatarUrl = null,
    bool IsBot = false,
    string? BotToken = null
);