using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class CreateDirectMessageCommandHandler(
    IConversationRepository conversationRepository,
    IChatProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateConversationResponse>> Handle(
        CreateDirectMessageCommand command,
        CancellationToken cancellationToken)
    {
        // Vérifier si un DM existe déjà entre ces deux profils
        var existing = await conversationRepository.GetDirectMessageAsync(
            command.InitiatorChatProfileId, command.RecipientChatProfileId, cancellationToken);

        if (existing is not null)
            return Result.Success(new CreateConversationResponse(existing.Id));

        var initiator = await profileRepository.GetByIdAsync(
            command.InitiatorChatProfileId, cancellationToken);
        if (initiator is null || !initiator.IsActive)
            return Result.Failure<CreateConversationResponse>("Initiator profile not found.");

        var recipient = await profileRepository.GetByIdAsync(
            command.RecipientChatProfileId, cancellationToken);
        if (recipient is null || !recipient.IsActive)
            return Result.Failure<CreateConversationResponse>("Recipient profile not found.");

        var result = Domain.Entities.Conversation.CreateDirect(
            command.InitiatorChatProfileId, command.RecipientChatProfileId);

        if (result.IsFailure)
            return Result.Failure<CreateConversationResponse>(result.Error);

        await conversationRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateConversationResponse(result.Value.Id));
    }
}
