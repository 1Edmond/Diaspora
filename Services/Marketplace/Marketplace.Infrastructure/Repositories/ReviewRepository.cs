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

    // IgnoreQueryFilters : l'index unique (ListingId, ReviewerId) s'applique aussi aux avis
    // supprimés (soft delete). Sans cela, un auteur ayant supprimé son avis passerait ce contrôle,
    // puis l'INSERT échouerait sur l'index unique (HTTP 500). Un avis supprimé compte donc comme
    // « déjà posté » et l'appelant reçoit une erreur métier claire.
    public async Task<bool> HasReviewedAsync(Guid listingId, Guid reviewerId, CancellationToken cancellationToken = default)
        => await _context.Reviews
            .IgnoreQueryFilters()
            .AnyAsync(r => r.ListingId == listingId && r.ReviewerId == reviewerId, cancellationToken);

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

    public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedReviewsAdminAsync(
        int page,
        int pageSize,
        int? rating = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews
            .Include(r => r.Listing)
            .AsNoTracking();

        if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(r => r.Comment.ToLower().Contains(s)
                || (r.Listing != null && r.Listing.Title.ToLower().Contains(s))
                || (r.ProviderReply != null && r.ProviderReply.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
