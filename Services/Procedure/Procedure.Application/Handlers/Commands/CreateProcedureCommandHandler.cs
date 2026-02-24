using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Procedure.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class CreateProcedureCommandHandler(
    IProcedureRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateProcedureResponse>> Handle(
        CreateProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(command.CostAmount, command.CostCurrency);
        if (moneyResult.IsFailure)
            return Result.Failure<CreateProcedureResponse>(moneyResult.Error);

        var procedureResult = Domain.Entities.Procedure.Create(
            command.Title,
            command.Description,
            moneyResult.Value,
            command.ProfileType,
            command.ProfileTypeId,
            command.EstimatedDurationDays,
            command.LocationId);

        if (procedureResult.IsFailure)
            return Result.Failure<CreateProcedureResponse>(procedureResult.Error);

        var procedure = procedureResult.Value;

        foreach (var depId in command.DependencyIds)
        {
            var depResult = procedure.AddDependency(depId);
            if (depResult.IsFailure)
                return Result.Failure<CreateProcedureResponse>(depResult.Error);
        }

        foreach (var docTypeId in command.RequiredDocumentTypeIds)
            procedure.AddRequiredDocument(docTypeId);

        await repository.AddAsync(procedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProcedureResponse(procedure.Id));
    }
}
