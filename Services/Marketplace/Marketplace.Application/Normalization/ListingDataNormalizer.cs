using Marketplace.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Marketplace.Application.Normalization;

public record UnrecognizedListingLocale(Guid ListingId, string? RawCountry, string? RawCurrency);

public record NormalizationReport(
    int TotalScanned,
    int TotalUpdated,
    IReadOnlyList<UnrecognizedListingLocale> UnrecognizedListings);

public interface IListingDataNormalizer
{
    (bool Recognized, string? Normalized) TryNormalizeCountry(string? rawCountry);
    (bool Recognized, string? Normalized) TryNormalizeCurrency(string? rawCurrency);
    NormalizationReport NormalizeListings(IEnumerable<ServiceListing> listings);
}

public class ListingDataNormalizer : IListingDataNormalizer
{
    private static readonly Regex Alpha2Regex = new("^[a-zA-Z]{2}$", RegexOptions.Compiled);
    private static readonly Regex Alpha3Regex = new("^[a-zA-Z]{3}$", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> CountryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["togo"] = "TG",
        ["tégé"] = "TG",
        ["benin"] = "BJ",
        ["bénin"] = "BJ",
        ["senegal"] = "SN",
        ["sénégal"] = "SN",
        ["cote d'ivoire"] = "CI",
        ["côte d'ivoire"] = "CI",
        ["ivory coast"] = "CI",
        ["ghana"] = "GH",
        ["nigeria"] = "NG",
        ["nigéria"] = "NG",
        ["niger"] = "NE",
        ["burkina faso"] = "BF",
        ["burkina"] = "BF",
        ["mali"] = "ML",
        ["guinee"] = "GN",
        ["guinée"] = "GN",
        ["guinea"] = "GN",
        ["cameroun"] = "CM",
        ["cameroon"] = "CM",
        ["gabon"] = "GA",
        ["congo"] = "CG",
        ["republique du congo"] = "CG",
        ["république du congo"] = "CG",
        ["rdc"] = "CD",
        ["drc"] = "CD",
        ["congo rdc"] = "CD",
        ["congo-kinshasa"] = "CD",
        ["france"] = "FR",
        ["russie"] = "RU",
        ["russia"] = "RU",
        ["usa"] = "US",
        ["united states"] = "US",
        ["etats-unis"] = "US",
        ["états-unis"] = "US",
        ["canada"] = "CA",
        ["belgique"] = "BE",
        ["belgium"] = "BE",
        ["suisse"] = "CH",
        ["switzerland"] = "CH",
        ["allemagne"] = "DE",
        ["germany"] = "DE",
        ["uk"] = "GB",
        ["royaume-uni"] = "GB",
        ["united kingdom"] = "GB"
    };

    private static readonly Dictionary<string, string> CurrencyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cfa"] = "XOF",
        ["fcfa"] = "XOF",
        ["f cfa"] = "XOF",
        ["franc cfa"] = "XOF",
        ["francs cfa"] = "XOF",
        ["xof"] = "XOF",
        ["xaf"] = "XAF",
        ["cfa beac"] = "XAF",
        ["fcfa beac"] = "XAF",
        ["euro"] = "EUR",
        ["euros"] = "EUR",
        ["eur"] = "EUR",
        ["€"] = "EUR",
        ["dollar"] = "USD",
        ["dollars"] = "USD",
        ["usd"] = "USD",
        ["$"] = "USD",
        ["rub"] = "RUB",
        ["rouble"] = "RUB",
        ["roubles"] = "RUB",
        ["ruble"] = "RUB",
        ["rubles"] = "RUB",
        ["₽"] = "RUB",
        ["cedi"] = "GHS",
        ["cedis"] = "GHS",
        ["ghs"] = "GHS",
        ["naira"] = "NGN",
        ["ngn"] = "NGN",
        ["cad"] = "CAD",
        ["c$"] = "CAD",
        ["chf"] = "CHF",
        ["gbp"] = "GBP",
        ["livre"] = "GBP",
        ["livre sterling"] = "GBP",
        ["£"] = "GBP"
    };

    public (bool Recognized, string? Normalized) TryNormalizeCountry(string? rawCountry)
    {
        if (string.IsNullOrWhiteSpace(rawCountry))
            return (true, null);

        var trimmed = rawCountry.Trim();

        if (CountryAliases.TryGetValue(trimmed, out var aliasIso))
            return (true, aliasIso);

        if (Alpha2Regex.IsMatch(trimmed))
            return (true, trimmed.ToUpperInvariant());

        return (false, rawCountry);
    }

    public (bool Recognized, string? Normalized) TryNormalizeCurrency(string? rawCurrency)
    {
        if (string.IsNullOrWhiteSpace(rawCurrency))
            return (true, null);

        var trimmed = rawCurrency.Trim();

        if (CurrencyAliases.TryGetValue(trimmed, out var aliasIso))
            return (true, aliasIso);

        if (Alpha3Regex.IsMatch(trimmed))
            return (true, trimmed.ToUpperInvariant());

        return (false, rawCurrency);
    }

    public NormalizationReport NormalizeListings(IEnumerable<ServiceListing> listings)
    {
        var unrecognized = new List<UnrecognizedListingLocale>();
        var totalScanned = 0;
        var totalUpdated = 0;

        foreach (var listing in listings)
        {
            totalScanned++;

            var (countryOk, normalizedCountry) = TryNormalizeCountry(listing.Country);
            var (currencyOk, normalizedCurrency) = TryNormalizeCurrency(listing.Currency);

            if (!countryOk || !currencyOk)
            {
                unrecognized.Add(new UnrecognizedListingLocale(
                    listing.Id,
                    countryOk ? null : listing.Country,
                    currencyOk ? null : listing.Currency));
            }

            var targetCountry = countryOk ? normalizedCountry : listing.Country;
            var targetCurrency = currencyOk ? normalizedCurrency : listing.Currency;

            if (targetCountry != listing.Country || targetCurrency != listing.Currency)
            {
                listing.NormalizeLocale(targetCountry, targetCurrency);
                totalUpdated++;
            }
        }

        return new NormalizationReport(totalScanned, totalUpdated, unrecognized.AsReadOnly());
    }
}
