using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class MarkAsReadCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        MarkAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure("Conversation not found.");

        var result = conversation.MarkAsRead(command.ChatProfileId);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}