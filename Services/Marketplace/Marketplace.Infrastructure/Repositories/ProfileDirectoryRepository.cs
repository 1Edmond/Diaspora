using JasperFx.Core;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using Shared.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.Infrastructure.Repositories;

public class ProfileDirectoryRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ProfileDirectoryEntry>> logger,
    IDistributedCache cache)
    : Repository<ProfileDirectoryEntry>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IProfileDirectoryRepository
{
    public async Task<ProfileDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.ProfileDirectory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, ProfileDirectoryEntry>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, ProfileDirectoryEntry>();

        var entries = await _context.ProfileDirectory
            .AsNoTracking()
            .Where(e => ids.Contains(e.UserId))
            .ToListAsync(cancellationToken);

        return entries.ToDictionary(e => e.UserId);
    }
}
