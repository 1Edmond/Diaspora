using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;

namespace Freelance.Infrastructure.Repositories;

public class JobReviewRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobReview>> logger,
    IDistributedCache cache)
    : Repository<JobReview>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobReviewRepository
{
    public async Task<IEnumerable<JobReview>> GetByJobApplicationAsync(Guid jobApplicationId, CancellationToken cancellationToken = default)
        => await _context.JobReviews
            .Where(r => r.JobApplicationId == jobApplicationId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobReview>> GetBySubjectAsync(Guid subjectId, ReputationRole role, CancellationToken cancellationToken = default)
        => await _context.JobReviews
            .Where(r => r.SubjectId == subjectId && r.RaterRole == (RaterRole)role)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasReviewedAsync(Guid jobApplicationId, Guid raterId, CancellationToken cancellationToken = default)
        => await _context.JobReviews
            .AnyAsync(r => r.JobApplicationId == jobApplicationId && r.RaterId == raterId, cancellationToken);
}