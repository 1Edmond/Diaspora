using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class ExternalProfileType : Entity
{
    public string CountryName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string PhoneIndicatif { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private ExternalProfileType() { }

    private ExternalProfileType(
        Guid id,
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency) : base(id)
    {
        Id = id;
        CountryName = countryName;
        CountryCode = countryCode;
        PhoneIndicatif = phoneIndicatif;
        Currency = currency;
        IsActive = true;
    }

    public static Result<ExternalProfileType> Create(
        Guid id,
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return Result.Failure<ExternalProfileType>("Country name is required.");

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            return Result.Failure<ExternalProfileType>("Country code must be a 2-letter ISO code.");

        if (string.IsNullOrWhiteSpace(phoneIndicatif))
            return Result.Failure<ExternalProfileType>("Phone indicatif is required.");

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<ExternalProfileType>("Currency is required.");

        return Result.Success(new ExternalProfileType(
            id,
            countryName.Trim(),
            countryCode.ToUpperInvariant(),
            phoneIndicatif.Trim(),
            currency.ToUpperInvariant()));
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void Update(string countryName, string phoneIndicatif, string currency)
    {
        CountryName = countryName;
        PhoneIndicatif = phoneIndicatif;
        Currency = currency;
    }
}