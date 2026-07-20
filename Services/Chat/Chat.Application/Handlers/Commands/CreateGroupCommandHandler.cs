using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateGroupCommandHandler(
    IConversationRepository conversationRepository,
    IChatProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateConversationResponse>> Handle(
        CreateGroupCommand command,
        CancellationToken cancellationToken)
    {
        var creator = await profileRepository.GetByIdAsync(
            command.CreatedByChatProfileId, cancellationToken);

        if (creator is null || !creator.IsActive)
            return Result.Failure<CreateConversationResponse>("Creator profile not found.");

        var result = Domain.Entities.Conversation.CreateGroup(
            command.Title, command.Description,
            command.AvatarUrl, command.CreatedByChatProfileId);

        if (result.IsFailure)
            return Result.Failure<CreateConversationResponse>(result.Error);

        var conversation = result.Value;

        // Ajouter les participants initiaux (hors créateur déjà ajouté)
        foreach (var participantId in command.InitialParticipantIds
            .Where(id => id != command.CreatedByChatProfileId))
        {
            var addResult = conversation.AddParticipant(participantId);
            if (addResult.IsFailure)
                return Result.Failure<CreateConversationResponse>(addResult.Error);
        }

        await conversationRepository.AddAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateConversationResponse(conversation.Id));
    }
}
