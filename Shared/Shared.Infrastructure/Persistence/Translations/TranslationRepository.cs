using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Persistence.Translations;

public class TranslationRepository<TOwner>(
    DbContext context,
    ILogger<TranslationRepository<TOwner>> logger) : ITranslationRepository<TOwner>
    where TOwner : class, ITranslatable
{
    protected readonly DbSet<Translation<TOwner>> DbSet = context.Set<Translation<TOwner>>();

    public virtual async Task<IReadOnlyCollection<Translation<TOwner>>> GetByOwnerIdAsync(
        Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(t => t.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<Translation<TOwner>?> GetByOwnerIdAndCultureAsync(
        Guid ownerId, string cultureCode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.OwnerId == ownerId && t.CultureCode == cultureCode.ToLowerInvariant(),
                cancellationToken);
    }

    public virtual async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Translation<TOwner>>>> GetByOwnerIdsAsync(
        IEnumerable<Guid> ownerIds, CancellationToken cancellationToken = default)
    {
        var ids = ownerIds.ToList();

        var translations = await DbSet
            .AsNoTracking()
            .Where(t => ids.Contains(t.OwnerId))
            .ToListAsync(cancellationToken);

        return translations
            .GroupBy(t => t.OwnerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<Translation<TOwner>>)g.ToList());
    }

    public virtual async Task AddAsync(Translation<TOwner> translation, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(translation, cancellationToken);

        logger.LogInformation(
            "Translation for {OwnerType} {OwnerId} ({Culture}) added",
            typeof(TOwner).Name, translation.OwnerId, translation.CultureCode);
    }

    public virtual async Task UpsertAsync(
        Guid ownerId, string cultureCode, Dictionary<string, string> fields,
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(
                t => t.OwnerId == ownerId && t.CultureCode == cultureCode.ToLowerInvariant(),
                cancellationToken);

        if (existing is not null)
        {
            foreach (var (key, value) in fields)
                existing.SetField(key, value);

            return;
        }

        var translation = Translation<TOwner>.Create(ownerId, cultureCode, fields);
        await DbSet.AddAsync(translation, cancellationToken);
    }

    public virtual async Task DeleteAsync(Translation<TOwner> translation, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(translation);
        await Task.CompletedTask;
    }
}
