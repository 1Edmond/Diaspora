using Procedure.Application.Commands;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Procedure.Application.Requests;
using Procedure.Application.Responses;
using Procedure.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class CreateProcedureCommandHandler(
    IProcedureRepository repository,
    ILocationRepository locationRepository)
{
    public async Task<Result<Guid>> Handle(
        CreateProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(command.CostAmount, command.CostCurrency);
        if (moneyResult.IsFailure)
            return Result.Failure<Guid>(moneyResult.Error);

        var procedureResult = Domain.Entities.Procedure.Create(
            command.Title, command.Description, moneyResult.Value,
            command.ProfileType, command.ProfileTypeId,
            command.EstimatedDurationDays);

        if (procedureResult.IsFailure)
            return Result.Failure<Guid>(procedureResult.Error);

        var procedure = procedureResult.Value;

        // ── Locations existantes ──────────────────────────────────────────────
        foreach (var locationId in command.LocationIds)
        {
            var result = procedure.AddLocation(locationId);
            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);
        }

        // ── Locations inline (créées à la volée) ──────────────────────────────
        foreach (var req in command.InlineLocations)
        {
            var location = req.BuildLocation();
            if (location.IsFailure)
                return Result.Failure<Guid>(location.Error);

            await locationRepository.AddAsync(location.Value, cancellationToken);

            var result = procedure.AddLocation(location.Value.Id);
            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);
        }

        // ── Dépendances ───────────────────────────────────────────────────────
        foreach (var dependencyId in command.DependencyIds)
        {
            var result = procedure.AddDependency(dependencyId);
            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);
        }

        foreach (var docTypeId in command.RequiredDocumentTypeIds)
            procedure.AddRequiredDocument(docTypeId);

        await repository.AddAsync(procedure, cancellationToken);
        return Result.Success(procedure.Id);
    }
   
}
