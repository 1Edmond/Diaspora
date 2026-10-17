using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Procedure.Application.Interfaces;
using Procedure.Infrastructure.Persistences;
using Shared.Domain.Enums;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Infrastructure.Repositories;

public sealed class ProcedureRepository(
    DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Domain.Entities.Procedure>> logger,
    IDistributedCache cache
) : Repository<Domain.Entities.Procedure>(context, encryptionService, keyManagement, metadataRepository, logger, cache),
    IProcedureRepository
{
    private readonly ProcedureDbContext _context = (ProcedureDbContext)context;

    public async Task<IEnumerable<Domain.Entities.Procedure>> GetByProfileTypeAsync(
        ProcedureProfileType profileType,
        Guid profileTypeId,
        CancellationToken cancellationToken = default) =>
        await _context.Procedures
            .AsNoTracking()
            .Include(x => x.Locations)
                .ThenInclude(pl => pl.Location)
            .Include(x => x.Dependencies)
            .Include(x => x.Documents)
            .Where(x => x.ProfileType == profileType
                     && x.ProfileTypeId == profileTypeId
                     && x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public override async Task<Domain.Entities.Procedure?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _context.Procedures
            .Include(x => x.Locations)
                .ThenInclude(pl => pl.Location)
            .Include(x => x.Dependencies)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Domain.Entities.Procedure?> GetWithDependenciesAsync(
        Guid procedureId,
        CancellationToken cancellationToken = default) =>
        await _context.Procedures
            .Include(x => x.Locations)
                .ThenInclude(pl => pl.Location)
            .Include(x => x.Dependencies)
                .ThenInclude(d => d.DependsOnProcedure)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == procedureId, cancellationToken);
}