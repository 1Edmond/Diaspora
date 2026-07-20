using Chat.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record AddParticipantCommand(
    Guid ConversationId,
    Guid RequestingChatProfileId,
    Guid ChatProfileId,
    ParticipantRole Role = ParticipantRole.Member
) : ICommand<Result>;
