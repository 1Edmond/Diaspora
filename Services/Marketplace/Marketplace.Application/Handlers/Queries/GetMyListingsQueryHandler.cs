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

public class GetMyListingsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ServiceListingSummaryDto>>> Handle(GetMyListingsQuery query)
    {
        var totalCount = await _context.ServiceListings
            .CountAsync(l => l.ProviderId == query.ProviderId);

        var listings = await _context.ServiceListings
            .Include(l => l.Category)
            .Where(l => l.ProviderId == query.ProviderId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var provider = await _profileDirectory.GetByUserIdAsync(query.ProviderId);

        var items = listings.Select(l => new ServiceListingSummaryDto(
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
            l.AverageRating,
            l.ReviewCount,
            l.ImageUrls.FirstOrDefault(),
            l.CreatedAt,
            l.City,
            l.Country,
            null,
            l.Status
        )).ToList();

        return Result.Success(new PagedList<ServiceListingSummaryDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}