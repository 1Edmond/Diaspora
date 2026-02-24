using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class CompleteUserProcedureCommandHandler(
    IUserProcedureRepository userProcedureRepository,
    IProcedureRepository procedureRepository,
    IServiceParameterRepository parameterRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        CompleteUserProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var userProcedure = await userProcedureRepository.GetByIdAsync(
            command.UserProcedureId, cancellationToken);
        if (userProcedure is null)
            return Result.Failure("User procedure not found.");

        // Lire le paramètre de configuration — dépendances bloquantes ?
        var dependenciesBlocking = await parameterRepository.GetBooleanAsync(
            ServiceParameter.DependenciesBlocking,
            defaultValue: true,
            cancellationToken);

        // Récupérer les statuts des procédures dépendantes
        var procedure = await procedureRepository.GetByIdAsync(
            userProcedure.ProcedureId, cancellationToken);

        IEnumerable<UserProcedureStatus> dependencyStatuses = [];

        if (procedure is not null && procedure.Dependencies.Any())
        {
            var depProcedureIds = procedure.Dependencies
                .Select(d => d.DependsOnProcedureId);

            var depUserProcedures = await userProcedureRepository.GetDependencyStatusesAsync(
                userProcedure.ProfileId, depProcedureIds, cancellationToken);

            dependencyStatuses = depUserProcedures.Select(up => up.Status);
        }

        var result = userProcedure.Complete(
            command.CompletedByUserId,
            command.CompletedByRole,
            command.Notes,
            dependenciesBlocking,
            dependencyStatuses);

        if (result.IsFailure)
            return result;

        await userProcedureRepository.UpdateAsync(userProcedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
