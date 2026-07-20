using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Requests;

public sealed record InlineLocationRequest(
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
    List<DayScheduleRequest> Schedule
);