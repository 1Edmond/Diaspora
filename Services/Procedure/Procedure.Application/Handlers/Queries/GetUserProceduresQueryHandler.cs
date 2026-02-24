using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetUserProceduresQueryHandler(IUserProcedureRepository repository)
{
    public async Task<Result<PagedList<UserProcedureDto>>> Handle(
        GetUserProceduresQuery query,
        CancellationToken cancellationToken)
    {
        var userProcedures = await repository.GetByProfileIdAsync(
            query.ProfileId, cancellationToken);

        var dtos = userProcedures.Select(up => up.ToDto());
        var paged = PagedList<UserProcedureDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}