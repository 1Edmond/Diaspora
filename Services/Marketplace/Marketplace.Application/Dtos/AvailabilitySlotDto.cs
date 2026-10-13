using System;

namespace Marketplace.Application.Dtos;

public record AvailabilitySlotDto(
    DayOfWeek Day,
    TimeSpan StartTime,
    TimeSpan EndTime
);