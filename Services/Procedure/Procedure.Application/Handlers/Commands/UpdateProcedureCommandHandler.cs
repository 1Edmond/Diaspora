using Procedure.Application.Commands;
using Procedure.Application.Extensions;
using Procedure.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class UpdateProcedureCommandHandler(
    IProcedureRepository repository,
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        UpdateProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var procedure = await repository.GetWithDependenciesAsync(
           command.ProcedureId, cancellationToken);

        if (procedure is null)
            return Result.Failure("Procedure not found.");

        var moneyResult = Money.Create(command.CostAmount, command.CostCurrency);
        if (moneyResult.IsFailure)
            return Result.Failure(moneyResult.Error);

        procedure.Update(command.Title, command.Description,
            moneyResult.Value, command.EstimatedDurationDays);

        // ── Rebuild locations ─────────────────────────────────────────────────
        procedure.ClearLocations();

        foreach (var locationId in command.LocationIds)
        {
            var result = procedure.AddLocation(locationId);
            if (result.IsFailure)
                return Result.Failure(result.Error);
        }

        foreach (var req in command.InlineLocations)
        {
            var location = req.BuildLocation();
            if (location.IsFailure)
                return Result.Failure(location.Error);

            await locationRepository.AddAsync(location.Value, cancellationToken);

            var result = procedure.AddLocation(location.Value.Id);
            if (result.IsFailure)
                return Result.Failure(result.Error);
        }

        // ── Rebuild dependencies ──────────────────────────────────────────────
        procedure.ClearDependencies();
        foreach (var dependencyId in command.DependencyIds)
        {
            var result = procedure.AddDependency(dependencyId);
            if (result.IsFailure)
                return Result.Failure(result.Error);
        }

        // ── Rebuild required documents ────────────────────────────────────────
        procedure.ClearRequiredDocuments();
        foreach (var docTypeId in command.RequiredDocumentTypeIds)
            procedure.AddRequiredDocument(docTypeId);

        await repository.UpdateAsync(procedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}