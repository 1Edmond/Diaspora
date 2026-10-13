using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;

namespace Freelance.Infrastructure.Repositories;

public class JobCategoryRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobCategory>> logger,
    IDistributedCache cache)
    : Repository<JobCategory>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobCategoryRepository
{
    public async Task<IEnumerable<JobCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.JobCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
}