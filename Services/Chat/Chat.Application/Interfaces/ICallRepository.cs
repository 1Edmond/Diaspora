using Chat.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Interfaces;

public interface ICallRepository : IRepository<Call>
{
    Task<IEnumerable<Call>> GetByConversationIdAsync(
        Guid conversationId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<Call?> GetActiveCallAsync(
        Guid conversationId, CancellationToken cancellationToken = default);

    Task<int> CountByConversationIdAsync(
        Guid conversationId, CancellationToken cancellationToken = default);
}

