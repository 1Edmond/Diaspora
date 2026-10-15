using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Procedure.Application.Interfaces;
using Procedure.Domain.Entities;
using Procedure.Infrastructure.Persistences;
using Shared.Application.Models;
using Shared.Domain.Enums;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Repositories;

public sealed class UserProcedureRepository(DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<UserProcedureRepository> logger,
    IDistributedCache cache)
    : Repository<UserProcedure>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IUserProcedureRepository
{
    private readonly ProcedureDbContext _context = (ProcedureDbContext)context;

    public async Task<IEnumerable<UserProcedure>> GetByProfileIdAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
        => await _context.UserProcedures
            .Include(x => x.Procedure)
                .ThenInclude(p => p.Locations)
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

    public async Task<PagedList<UserProcedure>> GetAllAdminAsync(
        UserProcedureStatus? status = null,
        Guid? procedureId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserProcedures
            .Include(x => x.Procedure)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (procedureId.HasValue)
            query = query.Where(x => x.ProcedureId == procedureId.Value);

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<UserProcedure>(items, totalCount, pageNumber, pageSize);
    }
}
