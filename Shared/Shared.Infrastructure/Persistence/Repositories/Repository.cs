using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Security.Abstractions;
using Shared.Security.Helpers;
using System.Linq.Expressions;
using System.Text.Json;

namespace Shared.Infrastructure.Persistence.Repositories;

public class Repository<T>(
    DbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<T>> logger,
    IDistributedCache cache) : IRepository<T> where T : AggregateRoot
{
    protected readonly DbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();
    protected readonly IEncryptionService EncryptionService = encryptionService;
    protected readonly IKeyManagementService KeyManagement = keyManagement;
    protected readonly IEncryptionMetadataRepository MetadataRepository = metadataRepository;
    protected readonly ILogger<Repository<T>> Logger = logger;
    protected readonly IDistributedCache Cache = cache;
    private readonly bool _hasSensitiveData = EncryptedValueObjectHelper.HasSensitiveData<T>();

    private readonly string _cachePrefix = context.GetType().Assembly.GetName().Name ?? "UnknownService";

    private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    protected string BuildCacheKey(string suffix)
        => $"{_cachePrefix}:{typeof(T).Name}:{suffix}";

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);

        if (entity is null)
            return null;

        if (_hasSensitiveData)
            await EncryptedValueObjectHelper.DecryptEntityAsync(entity, EncryptionService, cancellationToken);

        return entity;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await DbSet.ToListAsync(cancellationToken);

        if (_hasSensitiveData)
        {
            foreach (var entity in entities)
                await EncryptedValueObjectHelper.DecryptEntityAsync(entity, EncryptionService, cancellationToken);
        }

        return entities;
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var entity = await DbSet.AsNoTracking()
                                .FirstOrDefaultAsync(predicate, cancellationToken);

        if (entity is not null && _hasSensitiveData)
            await EncryptedValueObjectHelper.DecryptEntityAsync(entity, EncryptionService, cancellationToken);

        return entity;
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var data = await Context.Set<T>()
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (_hasSensitiveData)
        {
            foreach (var entity in data)
                await EncryptedValueObjectHelper.DecryptEntityAsync(entity, EncryptionService, cancellationToken);
        }

        return data;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (_hasSensitiveData)
        {
            await EncryptedValueObjectHelper.EncryptEntityAsync(
                entity,
                EncryptionService,
                KeyManagement,
                MetadataRepository,
                cancellationToken);
        }

        await DbSet.AddAsync(entity, cancellationToken);

        Logger.LogInformation(
            "Entity {EntityType} with id {Id} added (has sensitive data: {HasSensitiveData})",
            typeof(T).Name, entity.Id, _hasSensitiveData);

        return entity;
    }

    public virtual async Task AddRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        if (_hasSensitiveData)
        {
            foreach (var entity in entities)
            {
                await EncryptedValueObjectHelper.EncryptEntityAsync(
                    entity,
                    EncryptionService,
                    KeyManagement,
                    MetadataRepository,
                    cancellationToken);
            }
        }

        await DbSet.AddRangeAsync(entities, cancellationToken);

        Logger.LogInformation(
            "Entity {EntityType} batch of {Count} added (has sensitive data: {HasSensitiveData})",
            typeof(T).Name, entities.Count, _hasSensitiveData);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (_hasSensitiveData)
        {
            await EncryptedValueObjectHelper.EncryptEntityAsync(
                entity,
                EncryptionService,
                KeyManagement,
                MetadataRepository,
                cancellationToken);
        }

        DbSet.Update(entity);

        await InvalidateCacheAsync(entity.Id, cancellationToken);

        Logger.LogInformation("Entity {EntityType} with id {Id} updated", typeof(T).Name, entity.Id);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);

        await InvalidateCacheAsync(entity.Id, cancellationToken);

        Logger.LogInformation("Entity {EntityType} with id {Id} deleted", typeof(T).Name, entity.Id);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    private async Task<TItem?> GetFromCacheAsync<TItem>(string key, CancellationToken ct)
    {
        try
        {
            var bytes = await Cache.GetAsync(key, ct);
            if (bytes is null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<TItem>(bytes);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cache read failed for key {CacheKey}", key);
            return default;
        }
    }

    private async Task SetCacheAsync<TItem>(string key, TItem value, CancellationToken ct)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await Cache.SetAsync(key, bytes, DefaultCacheOptions, ct);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cache write failed for key {CacheKey}", key);
        }
    }

    private async Task InvalidateCacheAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await Cache.RemoveAsync(BuildCacheKey(id.ToString()), ct);
            await Cache.RemoveAsync(BuildCacheKey("all"), ct);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cache invalidation failed for {EntityType} {Id}", typeof(T).Name, id);
        }
    }
}