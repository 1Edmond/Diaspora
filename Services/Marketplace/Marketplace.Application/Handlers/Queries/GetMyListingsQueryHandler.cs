using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetMyListingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetMyListingsQuery query)
    {
        var totalCount = await _context.ServiceListings
            .CountAsync(l => l.ProviderId == query.ProviderId);

        var items = await _context.ServiceListings
            .Include(l => l.Category)
            .Where(l => l.ProviderId == query.ProviderId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new ServiceListingSummaryDto(
                l.Id, l.ProviderId, string.Empty, l.Category.Name, l.Title,
                l.PaymentMode, l.Price, l.Currency, l.AverageRating,
                l.ReviewCount, l.ImageUrls.FirstOrDefault(), l.CreatedAt
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}