using Chat.Application.Responses;
using Chat.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;


public sealed record StartCallCommand(
    Guid ConversationId,
    Guid InitiatorChatProfileId,
    CallType Type
) : ICommand<Result<StartCallResponse>>;