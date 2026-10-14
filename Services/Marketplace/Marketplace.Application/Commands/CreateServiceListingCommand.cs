using Marketplace.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Commands;
public record CreateServiceListingCommand(
    Guid ProviderId,
    Guid CategoryId,
    string Title,
    string Description,
    string? ContactInfo,
    PaymentMode PaymentMode,
    decimal? Price,
    string? Currency,
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null,
    string? DepartureCountryCode = null,
    string? DepartureCity = null,
    string? ArrivalCountryCode = null,
    string? ArrivalCity = null,
    DateTime? DepartureDate = null,
    DateTime? ArrivalDate = null,
    DateTime? PickupDeadline = null,
    decimal? CapacityKg = null,
    decimal? PricePerKg = null,
    string? TransportMode = null,
    string? FlightOrTransportNumber = null,
    string? ProhibitedItemsDescription = null,
    string? MeetingPoints = null
) : ICommand<Result<Guid>>;