using Procedure.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Queries;

public sealed record GetProceduresByProfileTypeQuery(
    ProcedureProfileType ProfileType,
    Guid ProfileTypeId,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<ProcedureDto>>>(PageNumber, PageSize);