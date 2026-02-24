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

public sealed class GetProceduresByProfileTypeQueryHandler(IProcedureRepository repository)
{
    public async Task<Result<PagedList<ProcedureDto>>> Handle(
        GetProceduresByProfileTypeQuery query,
        CancellationToken cancellationToken)
    {
        var procedures = await repository.GetByProfileTypeAsync(
            query.ProfileType, query.ProfileTypeId, cancellationToken);

        var dtos = procedures.Select(p => p.ToDto());
        var paged = PagedList<ProcedureDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}