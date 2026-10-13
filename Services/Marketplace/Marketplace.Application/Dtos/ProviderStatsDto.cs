using System;

namespace Marketplace.Application.Dtos;

public record ProviderStatsDto(
    int TotalListings,
    int ActiveListings,
    int TotalRequestsReceived,
    int AcceptedRequests,
    int CompletedRequests,
    double AcceptanceRatePercent,
    double? AverageResponseTimeHours,
    decimal TotalRevenueCompleted,
    double AverageRating,
    int TotalReviews,
    int TotalFavorites
);