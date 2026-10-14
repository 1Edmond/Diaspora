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

    public static Result<PhoneNumber> Create(string phoneNumber, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>("Phone number cannot be empty");

        if (string.IsNullOrWhiteSpace(countryCode))
            return Result.Failure<PhoneNumber>("Country code cannot be empty");

        phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

        if (phoneNumber.Length < 7 || phoneNumber.Length > 15)
            return Result.Failure<PhoneNumber>("Phone number must be between 7 and 15 digits");

        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d+$"))
            return Result.Failure<PhoneNumber>("Phone number must contain only digits");

        countryCode = countryCode.Trim();
        if (!countryCode.StartsWith("+"))
            countryCode = "+" + countryCode;

        return Result.Success(new PhoneNumber(phoneNumber, countryCode));
    }

    public static Result<PhoneNumber> Create(string fullPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullPhoneNumber))
            return Result.Failure<PhoneNumber>("Phone number cannot be empty");

        var clean = fullPhoneNumber.Trim().Replace(" ", "").Replace("-", "");
        if (!clean.StartsWith("+"))
            return Result.Failure<PhoneNumber>("International phone number must start with '+' followed by country code");

        // Strip '+' and digits only
        var digits = clean[1..];
        if (!System.Text.RegularExpressions.Regex.IsMatch(digits, @"^\d{7,15}$"))
            return Result.Failure<PhoneNumber>("Phone number must contain between 7 and 15 digits");

        int codeLength;
        if (digits.StartsWith("1") || digits.StartsWith("7"))
        {
            codeLength = 1;
        }
        else if (TwoDigitCountryCodes.Contains(digits[..Math.Min(2, digits.Length)]))
        {
            codeLength = 2;
        }
        else
        {
            codeLength = 3;
        }

        if (digits.Length <= codeLength)
            return Result.Failure<PhoneNumber>("Invalid phone number format");

        var countryCode = "+" + digits[..codeLength];
        var localNumber = digits[codeLength..];

        return Create(localNumber, countryCode);
    }

    private static readonly HashSet<string> TwoDigitCountryCodes = new(StringComparer.Ordinal)
    {
        "20", "27",
        "30", "31", "32", "33", "34", "36", "39",
        "40", "41", "43", "44", "45", "46", "47", "48", "49",
        "51", "52", "53", "54", "55", "56", "57", "58",
        "60", "61", "62", "63", "64", "65", "66",
        "81", "82", "84", "86",
        "90", "91", "92", "93", "94", "95", "98"
    };

    public string GetFullNumber() => $"{CountryCode}{Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }

    public override string ToString() => GetFullNumber();
}