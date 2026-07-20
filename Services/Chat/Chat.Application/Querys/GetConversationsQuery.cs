using Chat.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Querys;

public sealed record GetConversationsQuery(
    Guid ChatProfileId,
    int PageNumber = 1,
    int PageSize = 30
) : PagedQuery<Result<PagedList<ConversationDto>>>(PageNumber, PageSize);