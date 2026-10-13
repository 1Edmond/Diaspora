using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetServiceListingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetServiceListingsQuery query)
    {
        var q = _context.ServiceListings
            .Include(l => l.Category)
            .AsQueryable();

        if (query.ActiveOnly)
            q = q.Where(l => l.IsActive);

        if (query.Status.HasValue)
            q = q.Where(l => l.Status == query.Status.Value);

        if (query.CategoryId.HasValue)
            q = q.Where(l => l.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            q = q.Where(l => l.Title.ToLower().Contains(term) || l.Description.ToLower().Contains(term));
        }

        if (query.PaymentMode.HasValue)
            q = q.Where(l => l.PaymentMode == query.PaymentMode.Value);

        if (query.MinPrice.HasValue)
            q = q.Where(l => l.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(l => l.Price <= query.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(l => l.City != null && l.City.ToLower() == query.City.ToLower());

        if (!string.IsNullOrWhiteSpace(query.Country))
            q = q.Where(l => l.Country != null && l.Country.ToLower() == query.Country.ToLower());

        // Distance/disponibilité nécessitent un post-traitement en mémoire car AvailabilitySlots
        // est stocké en JSON (non traduisible en SQL) et le calcul de distance (Haversine) n'est
        // pas traduisible non plus. On matérialise le sous-ensemble filtré par les critères SQL,
        // puis on applique distance/disponibilité/tri avant la pagination.
        var needsInMemoryProcessing = query.AvailableNow
            || query.SortBy == ListingSortBy.Distance
            || (query.MaxDistanceKm.HasValue && query.UserLatitude.HasValue && query.UserLongitude.HasValue);

        if (!needsInMemoryProcessing)
        {
            var totalCount = await q.CountAsync();

            var sortedQuery = query.SortBy switch
            {
                ListingSortBy.Newest => q.OrderByDescending(l => l.CreatedAt),
                ListingSortBy.RatingDesc => q.OrderByDescending(l => l.AverageRating).ThenByDescending(l => l.CreatedAt),
                ListingSortBy.PriceAsc => q.OrderBy(l => l.Price),
                ListingSortBy.PriceDesc => q.OrderByDescending(l => l.Price),
                _ => q.OrderByDescending(l => l.AverageRating).ThenByDescending(l => l.CreatedAt)
            };

            var pagedItems = await sortedQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(l => new ServiceListingSummaryDto(
                    l.Id, l.ProviderId, string.Empty, l.Category.Name, l.Title,
                    l.PaymentMode, l.Price, l.Currency, l.AverageRating, l.ReviewCount,
                    l.ImageUrls.FirstOrDefault(), l.CreatedAt, l.City, l.Country, null, l.Status
                ))
                .ToListAsync();

            return Result.Success(new PagedList<ServiceListingSummaryDto>(pagedItems, totalCount, query.PageNumber, query.PageSize));
        }

        // Chemin en mémoire (distance et/ou disponibilité)
        var candidates = await q.ToListAsync();
        var now = DateTime.UtcNow;

        IEnumerable<(Marketplace.Domain.Entities.ServiceListing Listing, double? Distance)> withDistance = candidates
            .Select(l => (
                Listing: l,
                Distance: (query.UserLatitude.HasValue && query.UserLongitude.HasValue && l.Latitude.HasValue && l.Longitude.HasValue)
                    ? GeoUtils.DistanceKm(query.UserLatitude.Value, query.UserLongitude.Value, l.Latitude.Value, l.Longitude.Value)
                    : (double?)null
            ));

        if (query.AvailableNow)
            withDistance = withDistance.Where(x => x.Listing.IsAvailableAt(now));

        if (query.MaxDistanceKm.HasValue)
            withDistance = withDistance.Where(x => x.Distance.HasValue && x.Distance.Value <= query.MaxDistanceKm.Value);

        var ordered = query.SortBy switch
        {
            ListingSortBy.Distance => withDistance.OrderBy(x => x.Distance ?? double.MaxValue),
            ListingSortBy.Newest => withDistance.OrderByDescending(x => x.Listing.CreatedAt),
            ListingSortBy.PriceAsc => withDistance.OrderBy(x => x.Listing.Price),
            ListingSortBy.PriceDesc => withDistance.OrderByDescending(x => x.Listing.Price),
            _ => withDistance.OrderByDescending(x => x.Listing.AverageRating).ThenByDescending(x => x.Listing.CreatedAt)
        };

        var orderedList = ordered.ToList();
        var inMemoryTotalCount = orderedList.Count;

        var pagedInMemory = orderedList
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ServiceListingSummaryDto(
                x.Listing.Id, x.Listing.ProviderId, string.Empty, x.Listing.Category.Name, x.Listing.Title,
                x.Listing.PaymentMode, x.Listing.Price, x.Listing.Currency, x.Listing.AverageRating, x.Listing.ReviewCount,
                x.Listing.ImageUrls.FirstOrDefault(), x.Listing.CreatedAt, x.Listing.City, x.Listing.Country, x.Distance, x.Listing.Status
            ))
            .ToList();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(pagedInMemory, inMemoryTotalCount, query.PageNumber, query.PageSize));
    }
}