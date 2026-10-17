using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
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

public class GetServiceListingsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
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

        if (query.ProviderId.HasValue)
            q = q.Where(l => l.ProviderId == query.ProviderId.Value);

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
        {
            var countryIso = query.Country.Trim().ToUpperInvariant();
            q = q.Where(l => l.Country != null && l.Country == countryIso);
        }

        if (!string.IsNullOrWhiteSpace(query.DepartureCountry))
        {
            var depIso = query.DepartureCountry.Trim().ToUpperInvariant();
            q = q.Where(l => l.DepartureCountryCode == depIso);
        }

        if (!string.IsNullOrWhiteSpace(query.DepartureCity))
        {
            var depCity = query.DepartureCity.Trim().ToLower();
            q = q.Where(l => l.DepartureCity != null && l.DepartureCity.ToLower() == depCity);
        }

        if (!string.IsNullOrWhiteSpace(query.ArrivalCountry))
        {
            var arrIso = query.ArrivalCountry.Trim().ToUpperInvariant();
            q = q.Where(l => l.ArrivalCountryCode == arrIso);
        }

        if (!string.IsNullOrWhiteSpace(query.ArrivalCity))
        {
            var arrCity = query.ArrivalCity.Trim().ToLower();
            q = q.Where(l => l.ArrivalCity != null && l.ArrivalCity.ToLower() == arrCity);
        }

        if (query.MinRemainingKg.HasValue)
        {
            q = q.Where(l => l.RemainingKg != null && l.RemainingKg >= query.MinRemainingKg.Value);
        }

        if (query.TravelDate.HasValue)
        {
            var travelDateUtc = query.TravelDate.Value.Date;
            q = q.Where(l => l.DepartureDate != null && l.DepartureDate.Value.Date >= travelDateUtc);
        }

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

            var pagedListings = await sortedQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var providerIds = pagedListings.Select(l => l.ProviderId).Distinct();
            var profiles = await _profileDirectory.GetByUserIdsAsync(providerIds);

            var pagedItems = pagedListings
                .Select(l =>
                {
                    profiles.TryGetValue(l.ProviderId, out var p);
                    return new ServiceListingSummaryDto(
                        l.Id, l.ProviderId, p?.Id, p?.DisplayName ?? string.Empty, p?.IsVerified ?? false, p?.CountryCode,
                        l.Category.Name, l.Title, l.PaymentMode, l.Price, l.Currency, l.AverageRating, l.ReviewCount,
                        l.ImageUrls.FirstOrDefault(), l.CreatedAt, l.City, l.Country, null, l.Status,
                        l.DepartureCountryCode, l.DepartureCity, l.ArrivalCountryCode, l.ArrivalCity,
                        l.DepartureDate, l.ArrivalDate, l.RemainingKg, l.PricePerKg, l.TransportMode
                    );
                })
                .ToList();

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

        var pagedCandidates = orderedList
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var inMemoryProviderIds = pagedCandidates.Select(x => x.Listing.ProviderId).Distinct();
        var inMemoryProfiles = await _profileDirectory.GetByUserIdsAsync(inMemoryProviderIds);

        var pagedInMemory = pagedCandidates
            .Select(x =>
            {
                inMemoryProfiles.TryGetValue(x.Listing.ProviderId, out var p);
                return new ServiceListingSummaryDto(
                    x.Listing.Id, x.Listing.ProviderId, p?.Id, p?.DisplayName ?? string.Empty, p?.IsVerified ?? false, p?.CountryCode,
                    x.Listing.Category.Name, x.Listing.Title, x.Listing.PaymentMode, x.Listing.Price, x.Listing.Currency,
                    x.Listing.AverageRating, x.Listing.ReviewCount, x.Listing.ImageUrls.FirstOrDefault(), x.Listing.CreatedAt,
                    x.Listing.City, x.Listing.Country, x.Distance, x.Listing.Status,
                    x.Listing.DepartureCountryCode, x.Listing.DepartureCity, x.Listing.ArrivalCountryCode, x.Listing.ArrivalCity,
                    x.Listing.DepartureDate, x.Listing.ArrivalDate, x.Listing.RemainingKg, x.Listing.PricePerKg, x.Listing.TransportMode
                );
            })
            .ToList();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(pagedInMemory, inMemoryTotalCount, query.PageNumber, query.PageSize));
    }

    public Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(SearchServiceListingsQuery query)
        => Handle((GetServiceListingsQuery)query);
}