using Shared.Domain.Commons;
using System;
using System.Text.RegularExpressions;

namespace Marketplace.Application.Abstractions;

/// <summary>
/// Normalisation et validation du pays (ISO 3166-1 alpha-2) et de la devise (ISO 4217) d'un listing.
/// Remplace les chaînes libres (« Togo », « togo », « TG ») qui faussaient les filtres par pays.
/// </summary>
public static class ListingLocaleRules
{
    private static readonly Regex CountryPattern = new("^[A-Z]{2}$", RegexOptions.Compiled);
    private static readonly Regex CurrencyPattern = new("^[A-Z]{3}$", RegexOptions.Compiled);

    public static string? NormalizeCountry(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    public static string? NormalizeCurrency(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    public static readonly Dictionary<string, string[]> AllowedCurrenciesByCountry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TG"] = ["XOF", "EUR", "USD"],
        ["BJ"] = ["XOF", "EUR", "USD"],
        ["CI"] = ["XOF", "EUR", "USD"],
        ["SN"] = ["XOF", "EUR", "USD"],
        ["ML"] = ["XOF", "EUR", "USD"],
        ["NE"] = ["XOF", "EUR", "USD"],
        ["BF"] = ["XOF", "EUR", "USD"],
        ["FR"] = ["EUR", "USD"],
        ["US"] = ["USD"],
        ["CA"] = ["CAD", "USD"],
        ["RU"] = ["RUB", "EUR", "USD"],
        ["GH"] = ["GHS", "USD", "EUR"],
        ["NG"] = ["NGN", "USD", "EUR"]
    };

    public static Result ValidateCountry(string? normalizedCountry)
        => normalizedCountry is null || CountryPattern.IsMatch(normalizedCountry)
            ? Result.Success()
            : Result.Failure("Country must be an ISO 3166-1 alpha-2 code (e.g. TG, BJ, SN).");

    public static Result ValidateCurrency(string? normalizedCurrency)
        => normalizedCurrency is null || CurrencyPattern.IsMatch(normalizedCurrency)
            ? Result.Success()
            : Result.Failure("Currency must be an ISO 4217 code (e.g. XOF, XAF, GHS).");

    public static Result ValidateCountryAndCurrency(string? normalizedCountry, string? normalizedCurrency)
    {
        var countryCheck = ValidateCountry(normalizedCountry);
        if (countryCheck.IsFailure)
            return countryCheck;

        var currencyCheck = ValidateCurrency(normalizedCurrency);
        if (currencyCheck.IsFailure)
            return currencyCheck;

        if (!string.IsNullOrEmpty(normalizedCountry) && !string.IsNullOrEmpty(normalizedCurrency))
        {
            if (AllowedCurrenciesByCountry.TryGetValue(normalizedCountry, out var allowed))
            {
                if (!allowed.Contains(normalizedCurrency, StringComparer.OrdinalIgnoreCase))
                {
                    return Result.Failure($"Currency '{normalizedCurrency}' is not allowed for country '{normalizedCountry}'. Allowed: {string.Join(", ", allowed)}.");
                }
            }
        }

        return Result.Success();
    }
}
