using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetPendingListingsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetPendingListingsQuery query)
    {
        var q = _context.ServiceListings
            .Include(l => l.Category)
            .Where(l => l.Status == ListingStatus.Pending);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new ServiceListingSummaryDto(
                l.Id, l.ProviderId, string.Empty, l.Category.Name, l.Title,
                l.PaymentMode, l.Price, l.Currency, 0, 0,
                l.ImageUrls.FirstOrDefault(), l.CreatedAt
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}