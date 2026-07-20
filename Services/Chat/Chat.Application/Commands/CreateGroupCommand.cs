using Chat.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record CreateGroupCommand(
    string Title,
    string? Description,
    string? AvatarUrl,
    Guid CreatedByChatProfileId,
    List<Guid> InitialParticipantIds
) : ICommand<Result<CreateConversationResponse>>;