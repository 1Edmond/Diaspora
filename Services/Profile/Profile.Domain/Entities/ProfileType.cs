using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class ProfileType : Entity
{
    public string CountryName { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string PhoneIndicatif { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public string Flag { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private ProfileType() { }

    private ProfileType(
        Guid id,
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency,
        string? flag = null) : base(id)
    {
        Id = id;
        CountryName = countryName;
        CountryCode = countryCode;
        PhoneIndicatif = phoneIndicatif;
        Currency = currency;
        Flag = !string.IsNullOrWhiteSpace(flag) ? flag.Trim() : GetFlagEmoji(countryCode);
        IsActive = true;
    }

    public static ProfileType Create(
        Guid id,
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency,
        string? flag = null)
    {
        return new(id,
            countryName.Trim(),
            countryCode.ToUpperInvariant(),
            phoneIndicatif.Trim(),
            currency.ToUpperInvariant(),
            flag);
    }

    public void Update(
        string countryName,
        string countryCode,
        string phoneIndicatif,
        string currency,
        string? flag = null)
    {
        CountryName = countryName.Trim();
        CountryCode = countryCode.ToUpperInvariant();
        PhoneIndicatif = phoneIndicatif.Trim();
        Currency = currency.ToUpperInvariant();
        Flag = !string.IsNullOrWhiteSpace(flag) ? flag.Trim() : GetFlagEmoji(countryCode);
    }

    public static string GetFlagEmoji(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            return string.Empty;

        var upper = countryCode.ToUpperInvariant();
        if (upper[0] < 'A' || upper[0] > 'Z' || upper[1] < 'A' || upper[1] > 'Z')
            return string.Empty;

        int first = char.ConvertToUtf32(upper, 0) + 0x1F1E6 - 'A';
        int second = char.ConvertToUtf32(upper, 1) + 0x1F1E6 - 'A';
        return char.ConvertFromUtf32(first) + char.ConvertFromUtf32(second);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

}
