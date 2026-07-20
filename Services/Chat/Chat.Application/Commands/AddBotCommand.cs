using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record AddBotCommand(
    Guid ConversationId,
    Guid RequestingChatProfileId,
    Guid BotChatProfileId
) : ICommand<Result>;