using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Profile.Api.Controllers;
using Profile.Application.DTOs;
using Profile.Application.Interfaces;
using Profile.Application.Requests;
using Profile.Domain.Entities;
using Shared.Domain.ValueObjects;
using Xunit;

namespace Profile.UnitTests;

public class ProfileTypeTests
{
    [Theory]
    [InlineData("TG", "\U0001F1F9\U0001F1EC")] // 🇹🇬
    [InlineData("FR", "\U0001F1EB\U0001F1F7")] // 🇫🇷
    [InlineData("BJ", "\U0001F1E7\U0001F1EF")] // 🇧🇯
    [InlineData("CI", "\U0001F1E8\U0001F1EE")] // 🇨🇮
    public void GetFlagEmoji_WhenValidCountryCode_ShouldReturnCorrectRegionalIndicatorEmoji(string countryCode, string expectedEmoji)
    {
        var emoji = ProfileType.GetFlagEmoji(countryCode);
        emoji.Should().Be(expectedEmoji);
    }

    [Theory]
    [InlineData("")]
    [InlineData("T")]
    [InlineData("TGO")]
    [InlineData("12")]
    public void GetFlagEmoji_WhenInvalidCountryCode_ShouldReturnEmpty(string invalidCode)
    {
        var emoji = ProfileType.GetFlagEmoji(invalidCode);
        emoji.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithoutExplicitFlag_ShouldAutoGenerateFlagEmoji()
    {
        var id = Guid.NewGuid();
        var profileType = ProfileType.Create(id, "Togo", "TG", "+228", "XOF");

        profileType.Id.Should().Be(id);
        profileType.CountryName.Should().Be("Togo");
        profileType.CountryCode.Should().Be("TG");
        profileType.PhoneIndicatif.Should().Be("+228");
        profileType.Currency.Should().Be("XOF");
        profileType.Flag.Should().Be(ProfileType.GetFlagEmoji("TG"));
        profileType.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithExplicitFlag_ShouldUseProvidedFlag()
    {
        var id = Guid.NewGuid();
        var profileType = ProfileType.Create(id, "Togo", "TG", "+228", "XOF", "custom_flag");

        profileType.Flag.Should().Be("custom_flag");
    }

    [Fact]
    public async Task GetPublic_ShouldReturnActiveProfileTypesPublicly()
    {
        var repo = Substitute.For<IProfileTypeRepository>();
        var logger = Substitute.For<ILogger<ProfileTypeController>>();
        var types = new List<ProfileType>
        {
            ProfileType.Create(Guid.NewGuid(), "Togo", "TG", "+228", "XOF"),
            ProfileType.Create(Guid.NewGuid(), "France", "FR", "+33", "EUR")
        };
        repo.GetAllActiveAsync(Arg.Any<CancellationToken>()).Returns(types);

        var controller = new ProfileTypeController(repo, logger);
        var actionResult = await controller.GetPublic(CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ProfileTypeDto>>().Subject.ToList();

        dtos.Should().HaveCount(2);
        dtos[0].CountryCode.Should().Be("TG");
        dtos[0].Currency.Should().Be("XOF");
        dtos[0].Flag.Should().Be(ProfileType.GetFlagEmoji("TG"));
        dtos[1].CountryCode.Should().Be("FR");
        dtos[1].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Create_WhenCountryCodeAlreadyExists_ShouldReturnConflict()
    {
        var repo = Substitute.For<IProfileTypeRepository>();
        var logger = Substitute.For<ILogger<ProfileTypeController>>();
        var existing = ProfileType.Create(Guid.NewGuid(), "Togo", "TG", "+228", "XOF");
        repo.GetByCountryCodeAsync("TG", Arg.Any<CancellationToken>()).Returns(existing);

        var controller = new ProfileTypeController(repo, logger);
        var request = new CreateProfileTypeRequest("Togo Bis", "TG", "+228", "XOF");

        var result = await controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
        await repo.DidNotReceive().AddAsync(Arg.Any<ProfileType>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("+22890000000", "+228", "90000000")]
    [InlineData("+33612345678", "+33", "612345678")]
    [InlineData("+14155552671", "+1", "4155552671")]
    [InlineData("+229 97 00 00 00", "+229", "97000000")]
    public void PhoneNumber_CreateFromInternationalString_ShouldParseCountryCodeAndNumber(
        string fullNumber, string expectedCode, string expectedValue)
    {
        var result = PhoneNumber.Create(fullNumber);

        result.IsSuccess.Should().BeTrue();
        result.Value.CountryCode.Should().Be(expectedCode);
        result.Value.Value.Should().Be(expectedValue);
        result.Value.GetFullNumber().Should().Be($"{expectedCode}{expectedValue}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("0612345678")]
    [InlineData("+")]
    [InlineData("+abc")]
    public void PhoneNumber_CreateFromInvalidInternationalString_ShouldFail(string invalidNumber)
    {
        var result = PhoneNumber.Create(invalidNumber);

        result.IsFailure.Should().BeTrue();
    }
}
