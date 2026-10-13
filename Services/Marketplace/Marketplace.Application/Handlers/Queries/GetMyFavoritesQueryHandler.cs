using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetMyFavoritesQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetMyFavoritesQuery query)
    {
        var favoritesQuery = _context.Favorites
            .Include(f => f.Listing).ThenInclude(l => l.Category)
            .Where(f => f.UserId == query.UserId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await favoritesQuery.CountAsync();

        var items = await favoritesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => new ServiceListingSummaryDto(
                f.Listing.Id, f.Listing.ProviderId, string.Empty, f.Listing.Category.Name, f.Listing.Title,
                f.Listing.PaymentMode, f.Listing.Price, f.Listing.Currency, f.Listing.AverageRating,
                f.Listing.ReviewCount, f.Listing.ImageUrls.FirstOrDefault(), f.Listing.CreatedAt,
                f.Listing.City, f.Listing.Country, null
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}