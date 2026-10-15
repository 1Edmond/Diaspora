using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetAdminUserProceduresQueryHandler(IUserProcedureRepository repository)
{
    public async Task<Result<PagedList<UserProcedureDto>>> Handle(
        GetAdminUserProceduresQuery query,
        CancellationToken cancellationToken)
    {
        var pagedUserProcedures = await repository.GetAllAdminAsync(
            query.Status,
            query.ProcedureId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = pagedUserProcedures.Items.Select(up => up.ToDto()).ToList();
        var paged = new PagedList<UserProcedureDto>(
            dtos,
            pagedUserProcedures.TotalCount,
            pagedUserProcedures.PageNumber,
            pagedUserProcedures.PageSize);

        return Result.Success(paged);
    }
}
