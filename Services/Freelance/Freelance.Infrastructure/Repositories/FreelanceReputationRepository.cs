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

public class FreelanceReputationRepository(
    FreelanceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<FreelanceReputation>> logger,
    IDistributedCache cache)
    : Repository<FreelanceReputation>(_context, encryptionService, keyManagement, metadataRepository, logger, cache), IFreelanceReputationRepository
{
    public async Task<FreelanceReputation?> GetBySubjectAsync(Guid subjectId, ReputationRole role, CancellationToken cancellationToken = default)
        => await _context.FreelanceReputations
            .FirstOrDefaultAsync(r => r.SubjectId == subjectId && r.Role == role, cancellationToken);
}