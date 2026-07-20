using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class JoinCallCommandHandler(
    ICallRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        JoinCallCommand command,
        CancellationToken cancellationToken)
    {
        var call = await repository.GetByIdAsync(command.CallId, cancellationToken);
        if (call is null) return Result.Failure("Call not found.");

        var result = call.JoinParticipant(command.ChatProfileId);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(call, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
