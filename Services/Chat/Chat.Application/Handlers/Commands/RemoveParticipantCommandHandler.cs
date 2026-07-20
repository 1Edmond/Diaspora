using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class RemoveParticipantCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        RemoveParticipantCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure("Conversation not found.");

        // L'utilisateur peut se retirer lui-même ou un admin peut retirer quelqu'un
        var isSelf = command.RequestingChatProfileId == command.ChatProfileId;
        if (!isSelf && !conversation.IsAdmin(command.RequestingChatProfileId))
            return Result.Failure("Only admins can remove other participants.");

        var result = conversation.RemoveParticipant(command.ChatProfileId);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
