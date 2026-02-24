using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.ValueObjects;

public sealed record DaySchedule(
    DayOfWeek Day,
    bool IsClosed,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime
)
{
    public static DaySchedule Closed(DayOfWeek day)
        => new(day, IsClosed: true, null, null);

    public static DaySchedule Open(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime)
        => new(day, IsClosed: false, openTime, closeTime);
}