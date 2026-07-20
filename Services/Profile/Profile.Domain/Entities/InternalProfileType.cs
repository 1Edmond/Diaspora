using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class InternalProfileType : Entity
{
    public string CountryName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string PhoneIndicatif { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private InternalProfileType() { }

    private InternalProfileType(
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

    public static InternalProfileType Create(
        Guid id,
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency)

    {

        return new(id,
            countryName.Trim(),
            countryCode.ToUpperInvariant(),
            phoneIndicatif.Trim(),
            currency.ToUpperInvariant());
    }

}
