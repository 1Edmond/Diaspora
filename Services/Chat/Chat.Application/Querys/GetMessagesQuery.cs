using Chat.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Querys;

public sealed record GetMessagesQuery(
    Guid ConversationId,
    Guid RequestingChatProfileId,
    int PageNumber = 1,
    int PageSize = 50
) : PagedQuery<Result<PagedList<MessageDto>>>(PageNumber, PageSize);
