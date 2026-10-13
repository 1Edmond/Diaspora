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

public class WorkerJobPreferenceRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<WorkerJobPreference>> logger,
    IDistributedCache cache)
    : Repository<WorkerJobPreference>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IWorkerJobPreferenceRepository
{
    public async Task<WorkerJobPreference?> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
        => await _context.WorkerJobPreferences
            .FirstOrDefaultAsync(p => p.WorkerId == workerId, cancellationToken);
}