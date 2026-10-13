using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Domain.Enums;
using Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Repositories;

public sealed class ConversationRepository(DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Conversation>> logger,
    IDistributedCache cache)
    : Repository<Conversation>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IConversationRepository
{
    private readonly ChatDbContext _context = (ChatDbContext)context;

    public async Task<Conversation?> GetWithParticipantsAsync(
        Guid conversationId, CancellationToken cancellationToken = default)
        => await _context.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.ChatProfile)
            .Include(c => c.MentionGroups)
                .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

    public async Task<Conversation?> GetDirectMessageAsync(
        Guid chatProfileId1, Guid chatProfileId2,
        CancellationToken cancellationToken = default)
        => await _context.Conversations
            .Include(c => c.Participants)
            .Where(c => c.Type == ConversationType.DirectMessage
                && c.Participants.Any(p => p.ChatProfileId == chatProfileId1 && p.IsActive)
                && c.Participants.Any(p => p.ChatProfileId == chatProfileId2 && p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<Conversation>> GetByParticipantAsync(
        Guid chatProfileId, CancellationToken cancellationToken = default)
        => await _context.Conversations
            .Include(c => c.Participants)
            .Where(c => c.IsActive
                && c.Participants.Any(p => p.ChatProfileId == chatProfileId && p.IsActive))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Conversation>> GetCommunitiesAsync(
        CancellationToken cancellationToken = default)
        => await _context.Conversations
            .Where(c => c.Type == ConversationType.Community && c.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByParticipantAsync(
        Guid chatProfileId, CancellationToken cancellationToken = default)
        => await _context.Conversations
            .CountAsync(c => c.IsActive
                && c.Participants.Any(p => p.ChatProfileId == chatProfileId && p.IsActive),
                cancellationToken);
}
