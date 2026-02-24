using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.ValueObjects;

public sealed record OpeningHours(IReadOnlyList<DaySchedule> Schedule)
{
    public static OpeningHours Empty()
        => new(Enum.GetValues<DayOfWeek>()
            .Select(DaySchedule.Closed)
            .ToList());

    public DaySchedule? GetScheduleForDay(DayOfWeek day)
        => Schedule.FirstOrDefault(s => s.Day == day);

    public bool IsOpenOn(DayOfWeek day)
        => Schedule.Any(s => s.Day == day && !s.IsClosed);
}