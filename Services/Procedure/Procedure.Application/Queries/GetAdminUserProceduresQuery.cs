using Procedure.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;
using Shared.Domain.Enums;

namespace Procedure.Application.Queries;

public sealed record GetAdminUserProceduresQuery(
    UserProcedureStatus? Status = null,
    Guid? ProcedureId = null,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<UserProcedureDto>>>(PageNumber, PageSize);
