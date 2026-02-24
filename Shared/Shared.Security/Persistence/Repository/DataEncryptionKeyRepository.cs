using Microsoft.EntityFrameworkCore;
using Shared.Security.Abstractions;
using Shared.Security.Models;

namespace Shared.Security.Persistence.Repository;

public class DataEncryptionKeyRepository : IDataEncryptionKeyRepository
{
    private readonly DbContext _context;

    public DataEncryptionKeyRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<DataEncryptionKey?> GetActiveAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<DataEncryptionKey>()
            .FirstOrDefaultAsync(
                x => x.EntityId == entityId &&
                     x.EntityType == entityType &&
                     x.IsActive,
                cancellationToken);
    }

    public async Task<DataEncryptionKey?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<DataEncryptionKey>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }


    public async Task DeactivateAllAsync(
        string entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var activeKeys = await _context.Set<DataEncryptionKey>()
            .Where(x => x.EntityId == entityId && x.EntityType == entityType && x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var key in activeKeys)
        {
            key.IsActive = false;
            key.RotatedAt = DateTime.UtcNow;
        }

        _context.Set<DataEncryptionKey>().UpdateRange(activeKeys);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(
        DataEncryptionKey key,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<DataEncryptionKey>().AddAsync(key, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
