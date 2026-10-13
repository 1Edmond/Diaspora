using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;

namespace Freelance.Domain.Entities;

public class WorkerJobPreference : AggregateRoot
{
    public Guid WorkerId { get; private set; }

    private readonly List<Guid> _preferredCategoryIds = new();
    public IReadOnlyCollection<Guid> PreferredCategoryIds => _preferredCategoryIds.AsReadOnly();

    public string? PreferredCity { get; private set; }
    public string? PreferredCountry { get; private set; }
    public double? PreferredLatitude { get; private set; }
    public double? PreferredLongitude { get; private set; }
    public int? PreferredRadiusKm { get; private set; }

    public bool OnlyRemote { get; private set; }
    public bool EmailNotificationsEnabled { get; private set; }

    private WorkerJobPreference() { }

    public static WorkerJobPreference Create(Guid workerId)
    {
        return new WorkerJobPreference
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            EmailNotificationsEnabled = true
        };
    }

    public void SetCategories(IEnumerable<Guid> categoryIds)
    {
        _preferredCategoryIds.Clear();
        _preferredCategoryIds.AddRange(categoryIds.Distinct());
    }

    public void SetLocation(string? city, string? country, double? latitude, double? longitude, int? radiusKm)
    {
        PreferredCity = city;
        PreferredCountry = country;
        PreferredLatitude = latitude;
        PreferredLongitude = longitude;
        PreferredRadiusKm = radiusKm;
    }

    public void SetRemotePreference(bool onlyRemote)
    {
        OnlyRemote = onlyRemote;
    }

    public void SetEmailNotifications(bool enabled)
    {
        EmailNotificationsEnabled = enabled;
    }
}