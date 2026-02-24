using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Repositories;

public sealed class ExternalProfileRepository(
    ProfileDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ExternalProfile>> logger,
    IDistributedCache cache
    )
    : Repository<ExternalProfile>(
        context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), IExternalProfileRepository
{
    private readonly ProfileDbContext _context = context;

    public async Task<IEnumerable<ExternalProfile>> GetByInternalProfileIdAsync(
        Guid internalProfileId,
        CancellationToken cancellationToken = default)
        => await _context.ExternalProfiles
            .Include(x => x.ExternalProfileType)
            .Where(x => x.InternalProfileId == internalProfileId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ExternalProfile>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.ExternalProfiles
            .Include(x => x.ExternalProfileType)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<ExternalProfile?> GetByUserIdAndTypeAsync(
        Guid userId,
        Guid externalProfileTypeId,
        CancellationToken cancellationToken = default)
        => await _context.ExternalProfiles
            .Include(x => x.ExternalProfileType)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.ExternalProfileTypeId == externalProfileTypeId,
                cancellationToken);
}

