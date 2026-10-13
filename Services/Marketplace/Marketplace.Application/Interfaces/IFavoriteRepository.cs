using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Interfaces;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<Favorite?> GetByUserAndListingAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
}