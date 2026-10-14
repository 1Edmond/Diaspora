using Marketplace.Application.Dtos;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Queries;

public record SearchServiceListingsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null,
    PaymentMode? PaymentMode = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool ActiveOnly = true,
    string? City = null,
    string? Country = null,
    ListingSortBy SortBy = ListingSortBy.Relevance,
    double? UserLatitude = null,
    double? UserLongitude = null,
    double? MaxDistanceKm = null,
    bool AvailableNow = false,
    ListingStatus? Status = null,
    string? DepartureCountry = null,
    string? DepartureCity = null,
    string? ArrivalCountry = null,
    string? ArrivalCity = null,
    decimal? MinRemainingKg = null,
    DateTime? TravelDate = null
) : GetServiceListingsQuery(
    PageNumber,
    PageSize,
    CategoryId,
    SearchTerm,
    PaymentMode,
    MinPrice,
    MaxPrice,
    ActiveOnly,
    City,
    Country,
    SortBy,
    UserLatitude,
    UserLongitude,
    MaxDistanceKm,
    AvailableNow,
    Status,
    DepartureCountry,
    DepartureCity,
    ArrivalCountry,
    ArrivalCity,
    MinRemainingKg,
    TravelDate
);
