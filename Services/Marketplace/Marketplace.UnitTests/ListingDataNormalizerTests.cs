using FluentAssertions;
using Marketplace.Application.Abstractions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Normalization;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class ListingDataNormalizerTests
{
    private readonly ListingDataNormalizer _normalizer = new();

    [Theory]
    [InlineData("togo", "TG")]
    [InlineData("TOGO", "TG")]
    [InlineData("tg", "TG")]
    [InlineData("TG", "TG")]
    [InlineData("bénin", "BJ")]
    [InlineData("benin", "BJ")]
    [InlineData("sénégal", "SN")]
    [InlineData("senegal", "SN")]
    [InlineData("côte d'ivoire", "CI")]
    [InlineData("cote d'ivoire", "CI")]
    [InlineData("france", "FR")]
    [InlineData("FR", "FR")]
    [InlineData("russie", "RU")]
    [InlineData("RU", "RU")]
    public void TryNormalizeCountry_WhenKnownOrAlpha2_ShouldReturnTrueAndIso(string input, string expectedIso)
    {
        var (recognized, normalized) = _normalizer.TryNormalizeCountry(input);

        recognized.Should().BeTrue();
        normalized.Should().Be(expectedIso);
    }

    [Theory]
    [InlineData("Atlantis")]
    [InlineData("PaysInconnu")]
    [InlineData("123")]
    public void TryNormalizeCountry_WhenUnrecognized_ShouldReturnFalseAndOriginal(string input)
    {
        var (recognized, normalized) = _normalizer.TryNormalizeCountry(input);

        recognized.Should().BeFalse();
        normalized.Should().Be(input);
    }

    [Fact]
    public void TryNormalizeCountry_WhenNullOrWhitespace_ShouldReturnTrueAndNull()
    {
        var (r1, n1) = _normalizer.TryNormalizeCountry(null);
        r1.Should().BeTrue();
        n1.Should().BeNull();

        var (r2, n2) = _normalizer.TryNormalizeCountry("   ");
        r2.Should().BeTrue();
        n2.Should().BeNull();
    }

    [Theory]
    [InlineData("cfa", "XOF")]
    [InlineData("fcfa", "XOF")]
    [InlineData("f cfa", "XOF")]
    [InlineData("franc cfa", "XOF")]
    [InlineData("xof", "XOF")]
    [InlineData("XOF", "XOF")]
    [InlineData("xaf", "XAF")]
    [InlineData("euro", "EUR")]
    [InlineData("euros", "EUR")]
    [InlineData("€", "EUR")]
    [InlineData("dollar", "USD")]
    [InlineData("$", "USD")]
    [InlineData("rub", "RUB")]
    [InlineData("rouble", "RUB")]
    [InlineData("₽", "RUB")]
    public void TryNormalizeCurrency_WhenKnownOrAlpha3_ShouldReturnTrueAndIso(string input, string expectedIso)
    {
        var (recognized, normalized) = _normalizer.TryNormalizeCurrency(input);

        recognized.Should().BeTrue();
        normalized.Should().Be(expectedIso);
    }

    [Theory]
    [InlineData("Bitcoin")]
    [InlineData("UnknownCurrency")]
    [InlineData("12")]
    public void TryNormalizeCurrency_WhenUnrecognized_ShouldReturnFalseAndOriginal(string input)
    {
        var (recognized, normalized) = _normalizer.TryNormalizeCurrency(input);

        recognized.Should().BeFalse();
        normalized.Should().Be(input);
    }

    [Fact]
    public void TryNormalizeCurrency_WhenNullOrWhitespace_ShouldReturnTrueAndNull()
    {
        var (r1, n1) = _normalizer.TryNormalizeCurrency(null);
        r1.Should().BeTrue();
        n1.Should().BeNull();

        var (r2, n2) = _normalizer.TryNormalizeCurrency("   ");
        r2.Should().BeTrue();
        n2.Should().BeNull();
    }

    [Fact]
    public void NormalizeListings_ShouldUpdateRecognizedAndKeepUnrecognized()
    {
        var l1 = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title1", "Desc", null, PaymentMode.ContactOnly, null,
            currency: "cfa", city: "Lomé", country: "togo");

        var l2 = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title2", "Desc", null, PaymentMode.ContactOnly, null,
            currency: "euro", city: "Cotonou", country: "bénin");

        var l3 = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title3", "Desc", null, PaymentMode.ContactOnly, null,
            currency: "XOF", city: "Dakar", country: "SN"); // already normalized

        var l4 = ServiceListing.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Title4", "Desc", null, PaymentMode.ContactOnly, null,
            currency: "MysteryCoin", city: "Nowhere", country: "Atlantis");

        var report = _normalizer.NormalizeListings(new[] { l1, l2, l3, l4 });

        report.TotalScanned.Should().Be(4);
        report.TotalUpdated.Should().Be(2); // l1 and l2 were updated; l3 already normalized; l4 unchanged

        l1.Country.Should().Be("TG");
        l1.Currency.Should().Be("XOF");

        l2.Country.Should().Be("BJ");
        l2.Currency.Should().Be("EUR");

        l3.Country.Should().Be("SN");
        l3.Currency.Should().Be("XOF");

        // l4 unrecognised values preserved without deletion
        l4.Country.Should().Be("Atlantis");
        l4.Currency.Should().Be("MysteryCoin");

        report.UnrecognizedListings.Should().HaveCount(1);
        report.UnrecognizedListings[0].ListingId.Should().Be(l4.Id);
        report.UnrecognizedListings[0].RawCountry.Should().Be("Atlantis");
        report.UnrecognizedListings[0].RawCurrency.Should().Be("MysteryCoin");
    }
}
