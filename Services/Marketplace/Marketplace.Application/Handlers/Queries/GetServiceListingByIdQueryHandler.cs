using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetServiceListingByIdQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<ServiceListingDto>> Handle(GetServiceListingByIdQuery query)
    {
        var listing = await _context.ServiceListings
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == query.Id);

        if (listing is null)
            return Result.Failure<ServiceListingDto>("Listing not found.");

        var dto = new ServiceListingDto(
            listing.Id,
            listing.ProviderId,
            string.Empty, // Résolu via Profile Service
            listing.CategoryId,
            listing.Category.Name,
            listing.Title,
            listing.Description,
            listing.ContactInfo,
            listing.PaymentMode,
            listing.Price,
            listing.Currency,
            listing.Status,
            listing.IsActive,
            listing.ImageUrls,
            listing.AverageRating,
            listing.ReviewCount,
            listing.ViewCount,
            listing.CreatedAt
        );

        return Result.Success(dto);
    }
}