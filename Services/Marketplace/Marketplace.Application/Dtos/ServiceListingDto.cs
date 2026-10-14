using Marketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record ServiceListingDto(
    Guid Id,
    Guid ProviderId,
    Guid? ProviderProfileId,
    string ProviderName,
    bool ProviderIsVerified,
    string? ProviderCountryCode,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Description,
    string? ContactInfo,
    PaymentMode PaymentMode,
    decimal? Price,
    string? Currency,
    ListingStatus Status,
    bool IsActive,
    IReadOnlyCollection<string> ImageUrls,
    double AverageRating,
    int ReviewCount,
    int ViewCount,
    DateTime CreatedAt,
    string? City,
    string? Country,
    double? Latitude,
    double? Longitude,
    IReadOnlyCollection<AvailabilitySlotDto> AvailabilitySlots,
    bool IsAvailableNow,
    string? DepartureCountryCode = null,
    string? DepartureCity = null,
    string? ArrivalCountryCode = null,
    string? ArrivalCity = null,
    DateTime? DepartureDate = null,
    DateTime? ArrivalDate = null,
    DateTime? PickupDeadline = null,
    decimal? CapacityKg = null,
    decimal? RemainingKg = null,
    decimal? PricePerKg = null,
    string? TransportMode = null,
    string? FlightOrTransportNumber = null,
    string? ProhibitedItemsDescription = null,
    string? MeetingPoints = null
);