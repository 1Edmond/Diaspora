using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Procedure.Infrastructure.Persistences;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Repositories;

public sealed class UserProcedureRepository(ProcedureDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<UserProcedure>> logger,
    IDistributedCache cache)
    : Repository<UserProcedure>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IUserProcedureRepository
{
    private readonly ProcedureDbContext _context = context;

    public async Task<IEnumerable<UserProcedure>> GetByProfileIdAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
        => await _context.UserProcedures
            .Include(x => x.Procedure)
                .ThenInclude(p => p.Location)
            .Where(x => x.ProfileId == profileId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<UserProcedure?> GetByProfileAndProcedureAsync(
        Guid profileId,
        Guid procedureId,
        CancellationToken cancellationToken = default)
        => await _context.UserProcedures
            .Include(x => x.Procedure)
            .FirstOrDefaultAsync(
                x => x.ProfileId == profileId && x.ProcedureId == procedureId,
                cancellationToken);

    public async Task<IEnumerable<UserProcedure>> GetDependencyStatusesAsync(
        Guid profileId,
        IEnumerable<Guid> dependencyProcedureIds,
        CancellationToken cancellationToken = default)
        => await _context.UserProcedures
            .Where(x => x.ProfileId == profileId
                     && dependencyProcedureIds.Contains(x.ProcedureId))
            .ToListAsync(cancellationToken);
}
