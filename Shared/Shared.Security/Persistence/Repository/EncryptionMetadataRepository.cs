using Microsoft.EntityFrameworkCore;
using Shared.Security.Abstractions;
using Shared.Security.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Security.Persistence.Repository;


public class EncryptionMetadataRepository : IEncryptionMetadataRepository
{
    private readonly DbContext _context;

    public EncryptionMetadataRepository(DbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        EncryptionMetadataRecord metadata,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<EncryptionMetadataRecord>().AddAsync(metadata, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EncryptionMetadataRecord?> GetByPropertyAsync(
        string entityType,
        string entityId,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EncryptionMetadataRecord>()
            .FirstOrDefaultAsync(
                x => x.EntityType == entityType &&
                     x.EntityId == entityId &&
                     x.PropertyName == propertyName,
                cancellationToken);
    }

    public async Task<List<EncryptionMetadataRecord>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EncryptionMetadataRecord>()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EncryptionMetadataRecord>> GetByKeyVersionAsync(
        string keyVersion,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<EncryptionMetadataRecord>()
            .Where(x => x.KeyVersion == keyVersion)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        EncryptionMetadataRecord metadata,
        CancellationToken cancellationToken = default)
    {
        _context.Set<EncryptionMetadataRecord>().Update(metadata);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var metadataRecords = await GetByEntityAsync(entityType, entityId, cancellationToken);
        _context.Set<EncryptionMetadataRecord>().RemoveRange(metadataRecords);
        await _context.SaveChangesAsync(cancellationToken);
    }
}