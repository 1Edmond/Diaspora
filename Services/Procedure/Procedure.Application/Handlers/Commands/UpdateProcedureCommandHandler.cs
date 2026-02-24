using Procedure.Application.Commands;
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
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        UpdateProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var procedure = await repository.GetByIdAsync(command.ProcedureId, cancellationToken);
        if (procedure is null)
            return Result.Failure("Procedure not found.");

        var moneyResult = Money.Create(command.CostAmount, command.CostCurrency);
        if (moneyResult.IsFailure)
            return moneyResult;

        procedure.Update(command.Title, command.Description, moneyResult.Value,
            command.EstimatedDurationDays, command.LocationId);

        // Reconstruire les dépendances
        foreach (var existing in procedure.Dependencies.ToList())
            procedure.RemoveDependency(existing.DependsOnProcedureId);

        foreach (var depId in command.DependencyIds)
        {
            var depResult = procedure.AddDependency(depId);
            if (depResult.IsFailure)
                return depResult;
        }

        // Reconstruire les documents requis
        foreach (var existing in procedure.RequiredDocuments.ToList())
            procedure.RemoveRequiredDocument(existing.DocumentTypeId);

        foreach (var docTypeId in command.RequiredDocumentTypeIds)
            procedure.AddRequiredDocument(docTypeId);

        await repository.UpdateAsync(procedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}