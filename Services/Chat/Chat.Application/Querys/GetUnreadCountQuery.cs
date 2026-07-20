using Chat.Application.Dtos;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Querys;

public sealed record GetUnreadCountQuery(
    Guid ChatProfileId,
    Guid ConversationId
) : IQuery<Result<UnreadCountDto>>;