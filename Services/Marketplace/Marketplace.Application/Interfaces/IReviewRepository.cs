using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByListingAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<bool> HasReviewedAsync(Guid listingId, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<(double Average, int Count)> GetRatingStatsAsync(Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>Charge un avis avec son listing associé (nécessaire pour vérifier que l'appelant est bien le provider).</summary>
    Task<Review?> GetByIdWithListingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Nombre d'avis créés par cet utilisateur depuis la date donnée (anti-spam).</summary>
    Task<int> CountCreatedSinceAsync(Guid reviewerId, DateTime since, CancellationToken cancellationToken = default);
}
