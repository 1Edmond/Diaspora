using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class RemoveReactionCommandHandler(
    IMessageRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        RemoveReactionCommand command,
        CancellationToken cancellationToken)
    {
        var message = await repository.GetWithDetailsAsync(command.MessageId, cancellationToken);
        if (message is null) return Result.Failure("Message not found.");

        message.RemoveReaction(command.ChatProfileId, command.Emoji);

        await repository.UpdateAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
