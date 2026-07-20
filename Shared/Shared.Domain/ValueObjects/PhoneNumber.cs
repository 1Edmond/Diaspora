using Shared.Domain.Commons;
using Shared.Domain.Converters;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Domain.ValueObjects;

[JsonConverter(typeof(PhoneNumberJsonConverter))]
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; private set; }
    public string CountryCode { get; private set; }

    private PhoneNumber(string value, string countryCode)
    {
        Value = value;
        CountryCode = countryCode;
    }

    public static Result<PhoneNumber> Create(string phoneNumber, string countryCode = "+228")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>("Phone number cannot be empty");

        phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

        if (phoneNumber.Length < 7 || phoneNumber.Length > 15)
            return Result.Failure<PhoneNumber>("Phone number must be between 7 and 15 digits");

        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d+$"))
            return Result.Failure<PhoneNumber>("Phone number must contain only digits");

        return Result.Success(new PhoneNumber(phoneNumber, countryCode));
    }

    public string GetFullNumber() => $"{CountryCode}{Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }

    public override string ToString() => GetFullNumber();
}