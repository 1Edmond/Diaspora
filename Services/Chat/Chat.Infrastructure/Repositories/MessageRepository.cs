using Chat.Application.Interfaces;
using Chat.Domain.Entities;
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

public sealed class MessageRepository(ChatDbContext context,
     IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Message>> logger,
    IDistributedCache cache)
    : Repository<Message>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IMessageRepository
{
    private readonly ChatDbContext _context = context;

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(
        Guid conversationId, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Reactions)
            .Include(m => m.Mentions)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByConversationIdAsync(
        Guid conversationId, CancellationToken cancellationToken = default)
        => await _context.Messages
            .CountAsync(m => m.ConversationId == conversationId, cancellationToken);

    public async Task<int> CountUnreadAsync(
        Guid conversationId, Guid chatProfileId, DateTime? lastReadAt,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .Where(m => m.ConversationId == conversationId
                && !m.IsDeleted
                && m.SenderChatProfileId != chatProfileId);

        if (lastReadAt.HasValue)
            query = query.Where(m => m.CreatedAt > lastReadAt.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Message?> GetWithDetailsAsync(
        Guid messageId, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Reactions)
            .Include(m => m.Mentions)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
}
