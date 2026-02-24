using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Domain.Enums;
using Profile.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Repositories;

public sealed class InternalProfileRepository(
    ProfileDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<InternalProfile>> logger,
    IDistributedCache cache
    )
    : Repository<InternalProfile>(
        context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), IInternalProfileRepository
{
    private readonly ProfileDbContext _context = context;

    public async Task<InternalProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.InternalProfiles
            .Include(x => x.InternalProfileType)
            .Include(x => x.ExternalProfiles)
            .ThenInclude(x => x.ExternalProfileType)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.InternalProfiles
            .AnyAsync(x => x.UserId == userId, cancellationToken);

    public async Task<IEnumerable<InternalProfile>> GetPendingVerificationAsync(
        CancellationToken cancellationToken = default)
        => await _context.InternalProfiles
            .Where(x => x.Status == InternalProfileStatus.PendingVerification)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}
