using Procedure.Application.Commands;
using Procedure.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Handlers.Commands;

public sealed class SkipUserProcedureCommandHandler(
    IUserProcedureRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        SkipUserProcedureCommand command,
        CancellationToken cancellationToken)
    {
        var userProcedure = await repository.GetByIdAsync(
            command.UserProcedureId, cancellationToken);
        if (userProcedure is null)
            return Result.Failure("User procedure not found.");

        var result = userProcedure.Skip(command.Notes);
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(userProcedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
