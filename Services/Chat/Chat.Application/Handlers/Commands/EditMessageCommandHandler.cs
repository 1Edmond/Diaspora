using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class EditMessageCommandHandler(
    IMessageRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        EditMessageCommand command,
        CancellationToken cancellationToken)
    {
        var message = await repository.GetByIdAsync(command.MessageId, cancellationToken);
        if (message is null) return Result.Failure("Message not found.");

        var result = message.Edit(command.NewContent, command.RequestingChatProfileId);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
