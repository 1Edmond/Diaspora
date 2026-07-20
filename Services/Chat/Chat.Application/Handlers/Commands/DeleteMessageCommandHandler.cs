using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class DeleteMessageCommandHandler(
    IMessageRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeleteMessageCommand command,
        CancellationToken cancellationToken)
    {
        var message = await repository.GetByIdAsync(command.MessageId, cancellationToken);
        if (message is null) return Result.Failure("Message not found.");

        var result = message.Delete(command.RequestingChatProfileId, command.IsAdmin);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
