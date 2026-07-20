using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Application.Responses;
using Chat.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;

public sealed class StartCallCommandHandler(
    IConversationRepository conversationRepository,
    ICallRepository callRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<StartCallResponse>> Handle(
        StartCallCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null || !conversation.IsActive)
            return Result.Failure<StartCallResponse>("Conversation not found.");

        if (!conversation.HasParticipant(command.InitiatorChatProfileId))
            return Result.Failure<StartCallResponse>(
                "You are not a participant of this conversation.");

        // Vérifier qu'il n'y a pas déjà un appel actif
        var activeCall = await callRepository.GetActiveCallAsync(
            command.ConversationId, cancellationToken);

        if (activeCall is not null)
            return Result.Failure<StartCallResponse>("There is already an active call.");

        var result = Call.Create(
            command.ConversationId, command.InitiatorChatProfileId, command.Type);

        if (result.IsFailure)
            return Result.Failure<StartCallResponse>(result.Error);

        // Inviter tous les participants actifs
        foreach (var participant in conversation.Participants
            .Where(p => p.IsActive && p.ChatProfileId != command.InitiatorChatProfileId))
        {
            result.Value.AddParticipant(participant.ChatProfileId);
        }

        await callRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StartCallResponse(result.Value.Id));
    }
}
