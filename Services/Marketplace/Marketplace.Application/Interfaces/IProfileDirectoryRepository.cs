using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Marketplace.Application.Interfaces;

/// <summary>Accès à la projection locale des profils. <c>GetByIdAsync</c> prend un ProfileId.</summary>
public interface IProfileDirectoryRepository : IRepository<ProfileDirectoryEntry>
{
    Task<ProfileDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Résolution en lot (évite le N+1 pour les noms de prestataires). Clé = UserId.</summary>
    Task<IReadOnlyDictionary<Guid, ProfileDirectoryEntry>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
