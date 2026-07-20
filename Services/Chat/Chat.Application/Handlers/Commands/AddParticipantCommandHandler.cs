using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class AddParticipantCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        AddParticipantCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await repository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure("Conversation not found.");

        if (!conversation.IsAdmin(command.RequestingChatProfileId))
            return Result.Failure("Only admins can add participants.");

        var result = conversation.AddParticipant(command.ChatProfileId, command.Role);
        if (result.IsFailure) return result;

        await repository.UpdateAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
