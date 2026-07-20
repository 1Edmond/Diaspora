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

public class GetServiceListingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetServiceListingsQuery query)
    {
        var q = _context.ServiceListings
            .Include(l => l.Category)
            .AsQueryable();

        if (query.ActiveOnly)
            q = q.Where(l => l.IsActive);

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

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(l => l.AverageRating)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new ServiceListingSummaryDto(
                l.Id,
                l.ProviderId,
                string.Empty, // Résolu via Profile Service
                l.Category.Name,
                l.Title,
                l.PaymentMode,
                l.Price,
                l.Currency,
                l.AverageRating,
                l.ReviewCount,
                l.ImageUrls.FirstOrDefault(),
                l.CreatedAt
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}