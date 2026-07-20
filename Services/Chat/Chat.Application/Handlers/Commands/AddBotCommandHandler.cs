using Chat.Application.Commands;
using Chat.Application.Interfaces;
using Chat.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Commands;


public sealed class AddBotCommandHandler(
    IConversationRepository conversationRepository,
    IChatProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        AddBotCommand command,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetWithParticipantsAsync(
            command.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure("Conversation not found.");

        if (conversation.Type == ConversationType.DirectMessage)
            return Result.Failure("Cannot add bots to direct messages.");

        if (!conversation.IsAdmin(command.RequestingChatProfileId))
            return Result.Failure("Only admins can add bots.");

        var bot = await profileRepository.GetByIdAsync(
            command.BotChatProfileId, cancellationToken);

        if (bot is null || !bot.IsBot)
            return Result.Failure("Bot profile not found.");

        var result = conversation.AddParticipant(command.BotChatProfileId, ParticipantRole.Member);
        if (result.IsFailure) return result;

        await conversationRepository.UpdateAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}