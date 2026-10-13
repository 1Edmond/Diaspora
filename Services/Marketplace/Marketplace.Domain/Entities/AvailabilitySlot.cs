using System;

namespace Marketplace.Domain.Entities;

/// <summary>
/// Créneau de disponibilité hebdomadaire récurrent d'un prestataire pour un listing donné.
/// Stocké en JSON sur ServiceListing (pas une entité EF à part entière), comme ImageUrls.
/// Les heures sont exprimées en UTC.
/// </summary>
public class AvailabilitySlot
{
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public AvailabilitySlot() { }

    public AvailabilitySlot(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        Day = day;
        StartTime = startTime;
        EndTime = endTime;
    }

    public bool Contains(DateTime utcMoment)
    {
        if (utcMoment.DayOfWeek != Day) return false;
        var timeOfDay = utcMoment.TimeOfDay;
        return timeOfDay >= StartTime && timeOfDay <= EndTime;
    }
}