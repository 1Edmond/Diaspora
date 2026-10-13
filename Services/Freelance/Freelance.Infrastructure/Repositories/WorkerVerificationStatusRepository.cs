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

public class WorkerVerificationStatusRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<WorkerVerificationStatus>> logger,
    IDistributedCache cache)
    : Repository<WorkerVerificationStatus>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IWorkerVerificationStatusRepository
{
    public async Task<WorkerVerificationStatus?> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
        => await _context.WorkerVerificationStatuses
            .FirstOrDefaultAsync(s => s.WorkerId == workerId, cancellationToken);
}