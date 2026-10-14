using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetServiceListingByIdQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<ServiceListingDto>> Handle(GetServiceListingByIdQuery query)
    {
        var listing = await _context.ServiceListings
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == query.Id);

        if (listing is null)
            return Result.Failure<ServiceListingDto>("Listing not found.");

        var provider = await _profileDirectory.GetByUserIdAsync(listing.ProviderId);
        var now = DateTime.UtcNow;

        var dto = new ServiceListingDto(
            listing.Id,
            listing.ProviderId,
            provider?.Id,
            provider?.DisplayName ?? string.Empty,
            provider?.IsVerified ?? false,
            provider?.CountryCode,
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
            listing.CreatedAt,
            listing.City,
            listing.Country,
            listing.Latitude,
            listing.Longitude,
            listing.AvailabilitySlots
                .Select(s => new AvailabilitySlotDto(s.Day, s.StartTime, s.EndTime))
                .ToList(),
            listing.IsAvailableAt(now),
            listing.DepartureCountryCode,
            listing.DepartureCity,
            listing.ArrivalCountryCode,
            listing.ArrivalCity,
            listing.DepartureDate,
            listing.ArrivalDate,
            listing.PickupDeadline,
            listing.CapacityKg,
            listing.RemainingKg,
            listing.PricePerKg,
            listing.TransportMode,
            listing.FlightOrTransportNumber,
            listing.ProhibitedItemsDescription,
            listing.MeetingPoints
        );

        return Result.Success(dto);
    }
}