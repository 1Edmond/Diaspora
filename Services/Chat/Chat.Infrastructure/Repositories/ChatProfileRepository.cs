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

public sealed class ChatProfileRepository(ChatDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ChatProfile>> logger,
    IDistributedCache cache)
    : Repository<ChatProfile>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IChatProfileRepository
{
    private readonly ChatDbContext _context = context;

    public async Task<ChatProfile?> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _context.ChatProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task<ChatProfile?> GetByExternalProfileIdAsync(
        Guid externalProfileId, CancellationToken cancellationToken = default)
        => await _context.ChatProfiles
            .FirstOrDefaultAsync(p => p.ExternalProfileId == externalProfileId, cancellationToken);

    public async Task<bool> ExistsByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _context.ChatProfiles
            .AnyAsync(p => p.UserId == userId, cancellationToken);

    public async Task<IEnumerable<ChatProfile>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await _context.ChatProfiles
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
}
