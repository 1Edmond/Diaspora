using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Procedure.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;


public sealed class StartUserProcedureCommandHandler(
    IUserProcedureRepository userProcedureRepository,
    IProcedureRepository procedureRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<StartUserProcedureResponse>> Handle(
        StartUserProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var procedure = await procedureRepository.GetByIdAsync(
            command.ProcedureId, cancellationToken);
        if (procedure is null)
            return Result.Failure<StartUserProcedureResponse>("Procedure not found.");

        if (!procedure.IsActive)
            return Result.Failure<StartUserProcedureResponse>("Procedure is not active.");

        // Vérifier qu'il n'existe pas déjà
        var existing = await userProcedureRepository.GetByProfileAndProcedureAsync(
            command.ProfileId, command.ProcedureId, cancellationToken);
        if (existing is not null)
            return Result.Failure<StartUserProcedureResponse>(
                "This procedure has already been started.");

        var result = Domain.Entities.UserProcedure.Create(command.ProfileId, command.ProcedureId);
        if (result.IsFailure)
            return Result.Failure<StartUserProcedureResponse>(result.Error);

        var userProcedure = result.Value;

        // Start directement (passe NotStarted → InProgress)
        userProcedure.Start();

        await userProcedureRepository.AddAsync(userProcedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StartUserProcedureResponse(userProcedure.Id));
    }
}