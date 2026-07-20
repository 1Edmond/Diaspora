using Chat.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Interfaces;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetWithParticipantsAsync(
        Guid conversationId, CancellationToken cancellationToken = default);

    Task<Conversation?> GetDirectMessageAsync(
        Guid chatProfileId1, Guid chatProfileId2,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Conversation>> GetByParticipantAsync(
        Guid chatProfileId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Conversation>> GetCommunitiesAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountByParticipantAsync(
        Guid chatProfileId, CancellationToken cancellationToken = default);
}
