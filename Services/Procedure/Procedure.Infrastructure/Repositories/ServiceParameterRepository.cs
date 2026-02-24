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

public sealed class ServiceParameterRepository(ProcedureDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ServiceParameter>> logger,
    IDistributedCache cache)
    : Repository<ServiceParameter>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IServiceParameterRepository
{
    private readonly ProcedureDbContext _context = context;

    public async Task<ServiceParameter?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
        => await _context.ServiceParameters
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

    public async Task<bool> GetBooleanAsync(
        string key,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        var param = await GetByKeyAsync(key, cancellationToken);
        return param?.AsBoolean() ?? defaultValue;
    }
}