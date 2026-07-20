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

public sealed class GetConversationsQueryHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository)
{
    public async Task<Result<PagedList<ConversationDto>>> Handle(
        GetConversationsQuery query, CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository
            .GetByParticipantAsync(query.ChatProfileId, cancellationToken);

        var total = await conversationRepository
            .CountByParticipantAsync(query.ChatProfileId, cancellationToken);

        var dtos = new List<ConversationDto>();
        foreach (var conv in conversations)
        {
            var participant = conv.Participants
                .FirstOrDefault(p => p.ChatProfileId == query.ChatProfileId);

            var unread = await messageRepository.CountUnreadAsync(
                conv.Id, query.ChatProfileId,
                participant?.LastReadAt, cancellationToken);

            dtos.Add(conv.ToDto(unread));
        }

        var paged = PagedList<ConversationDto>.Create(
            dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}
