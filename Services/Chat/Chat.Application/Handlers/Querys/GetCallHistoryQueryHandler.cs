using Chat.Application.Dtos;
using Chat.Application.Interfaces;
using Chat.Application.Querys;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Handlers.Querys;

public sealed class GetCallHistoryQueryHandler(ICallRepository repository)
{
    public async Task<Result<PagedList<CallDto>>> Handle(
        GetCallHistoryQuery query, CancellationToken cancellationToken)
    {
        var calls = await repository.GetByConversationIdAsync(
            query.ConversationId, query.PageNumber, query.PageSize, cancellationToken);

        var total = await repository.CountByConversationIdAsync(
            query.ConversationId, cancellationToken);

        var dtos = calls.Select(c => new CallDto(
            c.Id, c.ConversationId, c.InitiatorChatProfileId,
            c.Type, c.Status, c.StartedAt, c.EndedAt, c.Duration,
            c.Participants.Count, c.CreatedAt)).ToList();

        var paged = PagedList<CallDto>.Create(dtos, query.PageNumber, query.PageSize);
        return Result.Success(paged);
    }
}