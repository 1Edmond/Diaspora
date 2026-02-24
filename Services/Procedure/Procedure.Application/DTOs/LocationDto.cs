using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.DTOs;

public sealed record LocationDto(
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
    IReadOnlyList<DayScheduleDto> Schedule
);