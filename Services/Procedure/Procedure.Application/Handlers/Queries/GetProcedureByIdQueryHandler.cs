using Procedure.Application.DTOs;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetProcedureByIdQueryHandler(IProcedureRepository repository)
{
    public async Task<Result<ProcedureDto>> Handle(
        GetProcedureByIdQuery query,
        CancellationToken cancellationToken)
    {
        var procedure = await repository.GetByIdAsync(query.ProcedureId, cancellationToken);
        if (procedure is null)
            return Result.Failure<ProcedureDto>("Procedure not found.");

        return Result.Success(procedure.ToDto());
    }
}