using FluentAssertions;
using Marketplace.Application.Abstractions;
using Xunit;

namespace Marketplace.UnitTests;

public class MarketplaceMultiCountryTests
{
    [Theory]
    [InlineData("tg", "TG")]
    [InlineData(" Togo ", "TOGO")]
    [InlineData(null, null)]
    [InlineData("  ", null)]
    public void NormalizeCountry_ShouldTrimAndUppercase(string? input, string? expected)
    {
        var result = ListingLocaleRules.NormalizeCountry(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("xof", "XOF")]
    [InlineData(" eur ", "EUR")]
    [InlineData(null, null)]
    [InlineData("  ", null)]
    public void NormalizeCurrency_ShouldTrimAndUppercase(string? input, string? expected)
    {
        var result = ListingLocaleRules.NormalizeCurrency(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("TG", true)]
    [InlineData("BJ", true)]
    [InlineData("CI", true)]
    [InlineData("FR", true)]
    [InlineData("US", true)]
    [InlineData(null, true)]
    [InlineData("TGO", false)]
    [InlineData("12", false)]
    [InlineData("T", false)]
    public void ValidateCountry_ShouldEnforceIsoAlpha2(string? country, bool shouldSucceed)
    {
        var result = ListingLocaleRules.ValidateCountry(country);
        result.IsSuccess.Should().Be(shouldSucceed);
    }

    [Theory]
    [InlineData("XOF", true)]
    [InlineData("EUR", true)]
    [InlineData("USD", true)]
    [InlineData("RUB", true)]
    [InlineData(null, true)]
    [InlineData("XOFA", false)]
    [InlineData("123", false)]
    [InlineData("US", false)]
    public void ValidateCurrency_ShouldEnforceIso4217(string? currency, bool shouldSucceed)
    {
        var result = ListingLocaleRules.ValidateCurrency(currency);
        result.IsSuccess.Should().Be(shouldSucceed);
    }

    [Fact]
    public void ValidateCountryAndCurrency_WhenCurrencyAllowedForCountry_ShouldSucceed()
    {
        var result = ListingLocaleRules.ValidateCountryAndCurrency("TG", "XOF");
        result.IsSuccess.Should().BeTrue();

        var eurResult = ListingLocaleRules.ValidateCountryAndCurrency("TG", "EUR");
        eurResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateCountryAndCurrency_WhenCurrencyNotAllowedForCountry_ShouldFailWithInformativeError()
    {
        var result = ListingLocaleRules.ValidateCountryAndCurrency("TG", "CAD");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Currency 'CAD' is not allowed for country 'TG'");
    }

    [Fact]
    public void ValidateCountryAndCurrency_WhenCountryFictitious_PureDataDrivenExtension()
    {
        // Seeding / registering a new fictitious country "WK" (Wakanda) dynamically in allowed map
        ListingLocaleRules.AllowedCurrenciesByCountry["WK"] = ["WKD", "USD"];

        var validResult = ListingLocaleRules.ValidateCountryAndCurrency("WK", "WKD");
        validResult.IsSuccess.Should().BeTrue();

        var invalidResult = ListingLocaleRules.ValidateCountryAndCurrency("WK", "EUR");
        invalidResult.IsFailure.Should().BeTrue();
    }
}
