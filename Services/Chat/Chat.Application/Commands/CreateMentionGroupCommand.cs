using Chat.Application.Responses;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Commands;

public sealed record CreateMentionGroupCommand(
    Guid ConversationId,
    Guid RequestingChatProfileId,
    string Name,
    List<Guid> MemberChatProfileIds
) : ICommand<Result<CreateMentionGroupResponse>>;