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
}
