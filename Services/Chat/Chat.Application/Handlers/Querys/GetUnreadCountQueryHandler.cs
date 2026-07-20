using Chat.Application.Dtos;
using Chat.Application.Interfaces;
using Chat.Application.Querys;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Querys;

public sealed class GetUnreadCountQueryHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository)
{
    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadCountQuery query, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetWithParticipantsAsync(
            query.ConversationId, cancellationToken);

        if (conversation is null)
            return Result.Failure<UnreadCountDto>("Conversation not found.");

        var participant = conversation.Participants
            .FirstOrDefault(p => p.ChatProfileId == query.ChatProfileId);

        if (participant is null)
            return Result.Failure<UnreadCountDto>("You are not a participant.");

        var count = await messageRepository.CountUnreadAsync(
            query.ConversationId, query.ChatProfileId,
            participant.LastReadAt, cancellationToken);

        return Result.Success(new UnreadCountDto(query.ConversationId, count));
    }
}
