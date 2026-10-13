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

public class JobTemplateRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<JobTemplate>> logger,
    IDistributedCache cache)
    : Repository<JobTemplate>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IJobTemplateRepository
{
    public async Task<IEnumerable<JobTemplate>> GetByEmployerAsync(Guid employerId, CancellationToken cancellationToken = default)
        => await _context.JobTemplates
            .Where(t => t.EmployerId == employerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
}