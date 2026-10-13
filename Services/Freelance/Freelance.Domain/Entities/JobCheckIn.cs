using Freelance.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

public class JobCheckIn : AggregateRoot
{
    public Guid JobApplicationId { get; private set; }
    public CheckInMethod Method { get; private set; }
    public CheckInStatus Status { get; private set; }

    public DateTime? CheckedInAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }

    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    public string? Notes { get; private set; }

    public JobApplication JobApplication { get; private set; } = null!;

    private JobCheckIn() { }

    public static JobCheckIn Create(Guid jobApplicationId, CheckInMethod method, double? latitude = null, double? longitude = null, string? notes = null)
    {
        return new JobCheckIn
        {
            Id = Guid.NewGuid(),
            JobApplicationId = jobApplicationId,
            Method = method,
            Status = CheckInStatus.Pending,
            Latitude = latitude,
            Longitude = longitude,
            Notes = notes
        };
    }

    public Result CheckIn(double? latitude = null, double? longitude = null)
    {
        if (Status != CheckInStatus.Pending)
            return Result.Failure("Check-in already processed.");

        Status = CheckInStatus.CheckedIn;
        CheckedInAt = DateTime.UtcNow;
        Latitude = latitude;
        Longitude = longitude;

        return Result.Success();
    }

    public Result CheckOut(double? latitude = null, double? longitude = null)
    {
        if (Status != CheckInStatus.CheckedIn)
            return Result.Failure("Cannot check out without checking in first.");

        Status = CheckInStatus.CheckedOut;
        CheckedOutAt = DateTime.UtcNow;
        Latitude = latitude;
        Longitude = longitude;

        return Result.Success();
    }

    public void MarkNoShow()
    {
        Status = CheckInStatus.NoShow;
    }
}