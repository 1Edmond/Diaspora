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

public sealed class ProfileRepository(
    DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<ProfileRepository> logger,
    IDistributedCache cache
    )
    : Repository<Domain.Entities.Profile>(
        context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), IProfileRepository
{
    private readonly ProfileDbContext _context = (ProfileDbContext)context;

    //public async Task<Domain.Entities.Profile?> GetByUserIdAsync(
    //    Guid userId,
    //    CancellationToken cancellationToken = default)
    //    => await _context.InternalProfiles
    //        .Include(x => x.ProfileType)
    //        .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.InternalProfiles
            .AnyAsync(x => x.UserId == userId, cancellationToken);

    public async Task<IEnumerable<Domain.Entities.Profile>> GetPendingVerificationAsync(
        CancellationToken cancellationToken = default)
        => await _context.InternalProfiles
            .Where(x => x.Status == ProfileStatus.PendingVerification)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Domain.Entities.Profile>?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
       return await _context.InternalProfiles
            .Include(x => x.ProfileType)
            .Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Profile>> GetBatchWithProfileTypeAsync(
        int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.InternalProfiles
            .AsNoTracking()
            .Include(x => x.ProfileType)
            .OrderBy(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InternalProfiles.CountAsync(cancellationToken);
    }
}
