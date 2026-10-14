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

    public static Result ValidateCountry(string? normalizedCountry)
        => normalizedCountry is null || CountryPattern.IsMatch(normalizedCountry)
            ? Result.Success()
            : Result.Failure("Country must be an ISO 3166-1 alpha-2 code (e.g. TG, BJ, SN).");

    public static Result ValidateCurrency(string? normalizedCurrency)
        => normalizedCurrency is null || CurrencyPattern.IsMatch(normalizedCurrency)
            ? Result.Success()
            : Result.Failure("Currency must be an ISO 4217 code (e.g. XOF, XAF, GHS).");
}
