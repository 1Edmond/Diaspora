using Procedure.Application.Requests;
using Procedure.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Application.Extensions;

public static class InlineLocationRequestExtensions
{
    public static Result<Location> BuildLocation(this InlineLocationRequest req)
    {
        var addressResult = Address.Create(
            req.Street, req.City, req.State, req.PostalCode, req.Country);

        if (addressResult.IsFailure)
            return Result.Failure<Location>(addressResult.Error);

        var schedule = req.Schedule.Select(s =>
            s.IsClosed
                ? DaySchedule.Closed(s.Day)
                : DaySchedule.Open(s.Day, s.OpenTime!.Value, s.CloseTime!.Value))
            .ToList();

        return Location.Create(
            req.Name, addressResult.Value,
            req.Latitude, req.Longitude,
            req.PhoneNumber, req.Website,
            new OpeningHours(schedule));
    }
}
