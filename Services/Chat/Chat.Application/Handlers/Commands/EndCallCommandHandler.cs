using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class EndCallCommandHandler(
    ICallRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        EndCallCommand command,
        CancellationToken cancellationToken)
    {
        var call = await repository.GetByIdAsync(command.CallId, cancellationToken);
        if (call is null) return Result.Failure("Call not found.");

        if (call.Status is CallStatus.Ended or CallStatus.Missed or CallStatus.Rejected)
            return Result.Failure("Call already ended.");

        // Leave ou End selon si c'est l'initiateur
        if (command.RequestingChatProfileId == call.InitiatorChatProfileId)
            call.End(command.FinalStatus);
        else
            call.LeaveParticipant(command.RequestingChatProfileId);

        await repository.UpdateAsync(call, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}