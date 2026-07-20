using Chat.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record CreateChatProfileCommand(
    Guid UserId,
    Guid? ExternalProfileId,
    string DisplayName,
    string? AvatarUrl = null,
    bool IsBot = false,
    string? BotToken = null
) : ICommand<Result<CreateChatProfileResponse>>;