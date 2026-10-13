using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetProviderStatsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<ProviderStatsDto>> Handle(GetProviderStatsQuery query)
    {
        var listings = await _context.ServiceListings
            .Where(l => l.ProviderId == query.ProviderId)
            .ToListAsync();

        var listingIds = listings.Select(l => l.Id).ToList();

        var requests = await _context.ServiceRequests
            .Where(r => r.ProviderId == query.ProviderId)
            .ToListAsync();

        var reviews = await _context.Reviews
            .Where(r => listingIds.Contains(r.ListingId) && !r.IsDeleted)
            .ToListAsync();

        var favoritesCount = await _context.Favorites
            .CountAsync(f => listingIds.Contains(f.ListingId));

        var totalListings = listings.Count;
        var activeListings = listings.Count(l => l.IsActive);

        var totalRequests = requests.Count;
        var acceptedRequests = requests.Count(r =>
            r.Status is ServiceRequestStatus.Accepted or ServiceRequestStatus.Completed);
        var completedRequests = requests.Count(r => r.Status == ServiceRequestStatus.Completed);

        var acceptanceRate = totalRequests == 0 ? 0 : Math.Round(100.0 * acceptedRequests / totalRequests, 1);

        var respondedRequests = requests.Where(r => r.AcceptedAt.HasValue).ToList();
        double? avgResponseHours = respondedRequests.Count == 0
            ? null
            : Math.Round(respondedRequests.Average(r => (r.AcceptedAt!.Value - r.CreatedAt).TotalHours), 1);

        var totalRevenue = requests
            .Where(r => r.Status == ServiceRequestStatus.Completed && r.PaymentStatus == PaymentStatus.Completed)
            .Sum(r => r.AgreedPrice ?? 0);

        var averageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);

        var dto = new ProviderStatsDto(
            totalListings,
            activeListings,
            totalRequests,
            acceptedRequests,
            completedRequests,
            acceptanceRate,
            avgResponseHours,
            totalRevenue,
            averageRating,
            reviews.Count,
            favoritesCount
        );

        return Result.Success(dto);
    }
}