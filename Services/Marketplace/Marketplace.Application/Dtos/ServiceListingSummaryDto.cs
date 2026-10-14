using Marketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record ServiceListingSummaryDto(
    Guid Id,
    Guid ProviderId,
    Guid? ProviderProfileId,
    string ProviderName,
    bool ProviderIsVerified,
    string? ProviderCountryCode,
    string CategoryName,
    string Title,
    PaymentMode PaymentMode,
    decimal? Price,
    string? Currency,
    double AverageRating,
    int ReviewCount,
    string? ThumbnailUrl,
    DateTime CreatedAt,
    string? City,
    string? Country,
    double? DistanceKm = null,
    ListingStatus Status = ListingStatus.Pending,
    string? DepartureCountryCode = null,
    string? DepartureCity = null,
    string? ArrivalCountryCode = null,
    string? ArrivalCity = null,
    DateTime? DepartureDate = null,
    DateTime? ArrivalDate = null,
    decimal? RemainingKg = null,
    decimal? PricePerKg = null,
    string? TransportMode = null
);