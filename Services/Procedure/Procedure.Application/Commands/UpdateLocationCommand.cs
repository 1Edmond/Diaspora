using Procedure.Application.DTOs;
using Procedure.Application.Requests;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;

namespace Procedure.Application.Commands;

public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    string? PhoneNumber,
    string? Website,
    List<DayScheduleRequest>? Schedule
) : ICommand<Result<LocationDto>>;
