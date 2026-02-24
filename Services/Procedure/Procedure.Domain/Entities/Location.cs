using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class Location : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Website { get; private set; }
    public OpeningHours OpeningHours { get; private set; } = OpeningHours.Empty();
    public bool IsActive { get; private set; }

    private Location() { }

    private Location(
        Guid id,
        string name,
        Address address,
        double latitude,
        double longitude,
        string? phoneNumber,
        string? website,
        OpeningHours openingHours) : base(id)
    {
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        PhoneNumber = phoneNumber;
        Website = website;
        OpeningHours = openingHours;
        IsActive = true;
    }

    public static Result<Location> Create(
        string name,
        Address address,
        double latitude,
        double longitude,
        string? phoneNumber,
        string? website,
        OpeningHours openingHours)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Location>("Location name is required.");

        if (latitude < -90 || latitude > 90)
            return Result.Failure<Location>("Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            return Result.Failure<Location>("Longitude must be between -180 and 180.");

        return Result.Success(new Location(
            Guid.NewGuid(), name, address, latitude, longitude,
            phoneNumber, website, openingHours));
    }

    public void Update(
        string name,
        Address address,
        double latitude,
        double longitude,
        string? phoneNumber,
        string? website,
        OpeningHours openingHours)
    {
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        PhoneNumber = phoneNumber;
        Website = website;
        OpeningHours = openingHours;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}


