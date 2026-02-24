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

public sealed class LocationRepository(ProcedureDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Location>> logger,
    IDistributedCache cache)
    : Repository<Location>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), ILocationRepository
{
    private readonly ProcedureDbContext _context = context;

    public async Task<IEnumerable<Location>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _context.Locations
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}