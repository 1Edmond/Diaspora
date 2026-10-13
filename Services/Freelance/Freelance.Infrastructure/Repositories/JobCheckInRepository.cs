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

public class JobCheckInRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobCheckIn>> logger,
    IDistributedCache cache)
    : Repository<JobCheckIn>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobCheckInRepository
{
    public async Task<IEnumerable<JobCheckIn>> GetByJobApplicationAsync(Guid jobApplicationId, CancellationToken cancellationToken = default)
        => await _context.JobCheckIns
            .Where(c => c.JobApplicationId == jobApplicationId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<JobCheckIn>> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default)
        => await _context.JobCheckIns
            .Include(c => c.JobApplication)
            .Where(c => c.JobApplication.WorkerId == workerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
}