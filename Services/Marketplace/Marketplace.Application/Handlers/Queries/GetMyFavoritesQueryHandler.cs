using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetMyFavoritesQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetMyFavoritesQuery query)
    {
        var favoritesQuery = _context.Favorites
            .Include(f => f.Listing).ThenInclude(l => l.Category)
            .Where(f => f.UserId == query.UserId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await favoritesQuery.CountAsync();

        var favorites = await favoritesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var providerIds = favorites.Select(f => f.Listing.ProviderId).Distinct().ToList();
        var providers = await _profileDirectory.GetByUserIdsAsync(providerIds);

        var items = favorites.Select(f =>
        {
            providers.TryGetValue(f.Listing.ProviderId, out var provider);
            return new ServiceListingSummaryDto(
                f.Listing.Id,
                f.Listing.ProviderId,
                provider?.Id,
                provider?.DisplayName ?? string.Empty,
                provider?.IsVerified ?? false,
                provider?.CountryCode,
                f.Listing.Category.Name,
                f.Listing.Title,
                f.Listing.PaymentMode,
                f.Listing.Price,
                f.Listing.Currency,
                f.Listing.AverageRating,
                f.Listing.ReviewCount,
                f.Listing.ImageUrls.FirstOrDefault(),
                f.Listing.CreatedAt,
                f.Listing.City,
                f.Listing.Country,
                null,
                f.Listing.Status
            );
        }).ToList();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}