using Marketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Dtos;

public record ServiceListingSummaryDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
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
    ListingStatus Status = ListingStatus.Pending
);