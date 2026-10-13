using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Repositories;

public class FavoriteRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Favorite>> logger,
    IDistributedCache cache)
    : Repository<Favorite>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), IFavoriteRepository
{
    public async Task<Favorite?> GetByUserAndListingAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
        => await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);
}