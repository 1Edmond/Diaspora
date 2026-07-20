using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record RemoveParticipantCommand(
    Guid ConversationId,
    Guid RequestingChatProfileId,
    Guid ChatProfileId
) : ICommand<Result>;