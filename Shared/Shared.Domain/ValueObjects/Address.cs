using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private Address() { }

    public static Result<Address> Create(
        string street,
        string city,
        string state,
        string postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street is required");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City is required");

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>("Country is required");

        return Result.Success(new Address
        {
            Street = street,
            City = city,
            State = state ?? string.Empty,
            PostalCode = postalCode ?? string.Empty,
            Country = country
        });
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}