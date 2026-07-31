using Shared.Domain.Interfaces;

namespace Shared.Infrastructure.Persistence.Translations;

public interface ITranslationRepository<TOwner> where TOwner : class, ITranslatable
{
    Task<IReadOnlyCollection<Translation<TOwner>>> GetByOwnerIdAsync(
        Guid ownerId, CancellationToken cancellationToken = default);

    Task<Translation<TOwner>?> GetByOwnerIdAndCultureAsync(
        Guid ownerId, string cultureCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<Translation<TOwner>>>> GetByOwnerIdsAsync(
        IEnumerable<Guid> ownerIds, CancellationToken cancellationToken = default);

    Task AddAsync(Translation<TOwner> translation, CancellationToken cancellationToken = default);

    Task UpsertAsync(
        Guid ownerId, string cultureCode, Dictionary<string, string> fields,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Translation<TOwner> translation, CancellationToken cancellationToken = default);
}
