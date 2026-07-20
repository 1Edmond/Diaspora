using Chat.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Querys;

public sealed record GetCallHistoryQuery(
    Guid ConversationId,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<CallDto>>>(PageNumber, PageSize);