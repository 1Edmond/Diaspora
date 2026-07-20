using Chat.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<IEnumerable<Message>> GetByConversationIdAsync(
        Guid conversationId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByConversationIdAsync(
        Guid conversationId, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(
        Guid conversationId, Guid chatProfileId, DateTime? lastReadAt,
        CancellationToken cancellationToken = default);

    Task<Message?> GetWithDetailsAsync(
        Guid messageId, CancellationToken cancellationToken = default);
}