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

public class JobApplicationRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobApplication>> logger,
    IDistributedCache cache)
    : Repository<JobApplication>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobApplicationRepository
{
    public override async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(a => a.Posting)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(a => a.Posting)
            .Where(a => a.WorkerId == workerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByJobPostingAsync(Guid jobPostingId, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Where(a => a.JobPostingId == jobPostingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(a => a.Posting)
            .Where(a => a.Posting.EmployerId == employerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobApplication>> GetByStatusAsync(Guid jobPostingId, JobApplicationStatus status, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Where(a => a.JobPostingId == jobPostingId && a.Status == status)
            .ToListAsync(cancellationToken);

    public async Task<int> CountCreatedSinceAsync(Guid workerId, DateTime since, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .CountAsync(a => a.WorkerId == workerId && a.CreatedAt >= since, cancellationToken);
}