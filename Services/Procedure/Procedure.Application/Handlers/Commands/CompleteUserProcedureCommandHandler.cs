using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Messaging.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class CompleteUserProcedureCommandHandler(
    IUserProcedureRepository userProcedureRepository,
    IProcedureRepository procedureRepository,
    ICacheService cacheService,
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

        var procedure = await procedureRepository.GetByIdAsync(
            userProcedure.ProcedureId, cancellationToken);

        if (procedure is null) return Result.Failure("Procedure not found.");
        IEnumerable<UserProcedureStatus> dependencyStatuses = [];

        if (procedure is not null && procedure.Dependencies.Any())
        {
            var depProcedureIds = procedure.Dependencies
                .Select(d => d.DependsOnProcedureId);

            var depUserProcedures = await userProcedureRepository.GetDependencyStatusesAsync(
                userProcedure.ProfileId, depProcedureIds, cancellationToken);

            dependencyStatuses = depUserProcedures.Select(up => up.Status);
        }

        var isAdmin = string.Equals(command.CompletedByRole, "Admin", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(command.CompletedByRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(command.CompletedByRole, "Officer", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && procedure!.IssuedDocuments.Any())
        {
            var values = await cacheService.GetAsync<Dictionary<Guid, List<Guid>>>(
                CachedKeyConstants.UpdloadDocumentIdForAwaitingTaskCompletedForKey,
                cancellationToken: cancellationToken);

            if (values is null || !values.TryGetValue(userProcedure.ProfileId, out var userDocs) || userDocs is null)
                return Result.Failure("Output document not found.");

            foreach (var proc in procedure.IssuedDocuments)
            {
                if (!userDocs.Any(d => d == proc.DocumentTypeId || d == proc.Id))
                    return Result.Failure("Output document not found.");
            }
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
