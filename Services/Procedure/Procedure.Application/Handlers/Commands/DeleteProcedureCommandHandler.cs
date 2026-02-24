using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class DeleteProcedureCommandHandler(
    IProcedureRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeleteProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var procedure = await repository.GetByIdAsync(command.ProcedureId, cancellationToken);
        if (procedure is null)
            return Result.Failure("Procedure not found.");

        procedure.Deactivate();

        await repository.UpdateAsync(procedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
