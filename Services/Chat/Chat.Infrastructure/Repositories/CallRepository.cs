using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Domain.Enums;
using Chat.Infrastructure.Persistence;
using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Repositories;

public sealed class CallRepository(ChatDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Call>> logger,
    IDistributedCache cache
    )
    : Repository<Call>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), ICallRepository
{
    private readonly ChatDbContext _context = context;

    public async Task<IEnumerable<Call>> GetByConversationIdAsync(
        Guid conversationId, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => await _context.Calls
            .Include(c => c.Participants)
                .ThenInclude(p => p.ChatProfile)
            .Where(c => c.ConversationId == conversationId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<Call?> GetActiveCallAsync(
        Guid conversationId, CancellationToken cancellationToken = default)
        => await _context.Calls
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(
                c => c.ConversationId == conversationId
                    && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Active),
                cancellationToken);

    public async Task<int> CountByConversationIdAsync(
        Guid conversationId, CancellationToken cancellationToken = default)
        => await _context.Calls
            .CountAsync(c => c.ConversationId == conversationId, cancellationToken);
}
