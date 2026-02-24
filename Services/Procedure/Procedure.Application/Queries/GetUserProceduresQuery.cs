using Procedure.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Queries;

public sealed record GetUserProceduresQuery(
    Guid ProfileId,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<UserProcedureDto>>>(PageNumber, PageSize);