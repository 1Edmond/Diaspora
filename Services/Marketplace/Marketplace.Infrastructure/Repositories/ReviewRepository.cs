using JasperFx.Core;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using Shared.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Repositories;

public class ReviewRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Review>> logger,
    IDistributedCache cache)
    : Repository<Review>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IReviewRepository
{
    public async Task<IEnumerable<Review>> GetByListingAsync(Guid listingId, CancellationToken cancellationToken = default)
        => await _context.Reviews
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasReviewedAsync(Guid listingId, Guid reviewerId, CancellationToken cancellationToken = default)
        => await _context.Reviews.AnyAsync(r => r.ListingId == listingId && r.ReviewerId == reviewerId, cancellationToken);

    public async Task<(double Average, int Count)> GetRatingStatsAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ListingId == listingId && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!reviews.Any()) return (0, 0);

        return (reviews.Average(r => r.Rating), reviews.Count);
    }

    public async Task<Review?> GetByIdWithListingAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Reviews
            .Include(r => r.Listing)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<int> CountCreatedSinceAsync(Guid reviewerId, DateTime since, CancellationToken cancellationToken = default)
        => await _context.Reviews
            .CountAsync(r => r.ReviewerId == reviewerId && r.CreatedAt >= since, cancellationToken);
}