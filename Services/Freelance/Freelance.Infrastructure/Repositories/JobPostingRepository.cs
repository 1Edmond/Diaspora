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

public class JobPostingRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobPosting>> logger,
    IDistributedCache cache)
    : Repository<JobPosting>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobPostingRepository
{
    public async Task<IEnumerable<JobPosting>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default)
        => await _context.JobPostings
            .Where(p => p.EmployerId == employerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobPosting>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await _context.JobPostings
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<int> CountCreatedSinceAsync(Guid employerId, DateTime since, CancellationToken cancellationToken = default)
        => await _context.JobPostings
            .CountAsync(p => p.EmployerId == employerId && p.CreatedAt >= since, cancellationToken);
}