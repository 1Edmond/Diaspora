using Procedure.Application.DTOs;
using Procedure.Application.Interfaces;
using Procedure.Application.Queries;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Queries;

public sealed class GetProcedureProgressQueryHandler(IUserProcedureRepository repository)
{
    public async Task<Result<ProcedureProgressDto>> Handle(
        GetProcedureProgressQuery query,
        CancellationToken cancellationToken)
    {
        var userProcedures = await repository.GetByProfileIdAsync(
            query.ProfileId, cancellationToken);

        var list = userProcedures.ToList();

        var total = list.Count;
        var completed = list.Count(up => up.Status == UserProcedureStatus.Completed);
        var inProgress = list.Count(up => up.Status == UserProcedureStatus.InProgress);
        var notStarted = list.Count(up => up.Status == UserProcedureStatus.NotStarted);
        var skipped = list.Count(up => up.Status == UserProcedureStatus.Skipped);

        var percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);

        return Result.Success(new ProcedureProgressDto(
            query.ProfileId, total, completed, inProgress, notStarted, skipped, percentage));
    }
}
