using Chat.Application.Dtos;
using Chat.Application.Extensions;
using Chat.Application.Interfaces;
using Chat.Application.Querys;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Querys;

public sealed class GetMessagesQueryHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IChatProfileRepository profileRepository)
{
    public async Task<Result<PagedList<MessageDto>>> Handle(
        GetMessagesQuery query, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetWithParticipantsAsync(
            query.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure<PagedList<MessageDto>>("Conversation not found.");

        if (!conversation.HasParticipant(query.RequestingChatProfileId))
            return Result.Failure<PagedList<MessageDto>>(
                "You are not a participant of this conversation.");

        var messages = await messageRepository.GetByConversationIdAsync(
            query.ConversationId, query.PageNumber, query.PageSize, cancellationToken);

        var total = await messageRepository.CountByConversationIdAsync(
            query.ConversationId, cancellationToken);

        // Charger les profils des expéditeurs
        var senderIds = messages.Select(m => m.SenderChatProfileId).Distinct();
        var profiles = (await profileRepository.GetByIdsAsync(senderIds, cancellationToken))
            .ToDictionary(p => p.Id);

        var dtos = messages
            .Select(m =>
            {
                profiles.TryGetValue(m.SenderChatProfileId, out var sender);
                return m.ToDto(
                    sender?.DisplayName ?? "Unknown",
                    sender?.AvatarUrl,
                    query.RequestingChatProfileId);
            })
            .ToList();

        var paged = PagedList<MessageDto>.Create(dtos, query.PageNumber, query.PageSize);
        return Result.Success(paged);
    }
}
