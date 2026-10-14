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

public class GetPendingListingsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetPendingListingsQuery query)
    {
        var q = _context.ServiceListings
            .Include(l => l.Category)
            .Where(l => l.Status == ListingStatus.Pending);

        var totalCount = await q.CountAsync();

        var listings = await q
            .OrderBy(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var providerIds = listings.Select(l => l.ProviderId).Distinct().ToList();
        var providers = await _profileDirectory.GetByUserIdsAsync(providerIds);

        var items = listings.Select(l =>
        {
            providers.TryGetValue(l.ProviderId, out var provider);
            return new ServiceListingSummaryDto(
                l.Id,
                l.ProviderId,
                provider?.Id,
                provider?.DisplayName ?? string.Empty,
                provider?.IsVerified ?? false,
                provider?.CountryCode,
                l.Category.Name,
                l.Title,
                l.PaymentMode,
                l.Price,
                l.Currency,
                0,
                0,
                l.ImageUrls.FirstOrDefault(),
                l.CreatedAt,
                l.City,
                l.Country,
                null,
                l.Status
            );
        }).ToList();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}