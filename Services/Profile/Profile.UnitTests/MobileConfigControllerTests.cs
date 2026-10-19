using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Profile.Api.Controllers;
using Profile.Application.DTOs;
using Profile.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Messaging.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Profile.UnitTests;

public class MobileConfigControllerTests
{
    [Fact]
    public async Task GetMobileConfig_ShouldReturnConfigWithParcelTransportIdAndActiveModules()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["MobileConfig:DefaultLocale"] = "fr",
            ["MobileConfig:MinAppVersion"] = "1.0.0"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var configRepo = Substitute.For<IMobileConfigRepository>();
        var profileTypeRepo = Substitute.For<IProfileTypeRepository>();
        var profileRepo = Substitute.For<IProfileRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var logger = Substitute.For<ILogger<MobileConfigController>>();
        var controller = new MobileConfigController(configRepo, profileTypeRepo, profileRepo, configuration, currentUserService, logger);

        // Act
        var result = await controller.GetMobileConfig();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<MobileBootstrapConfigDto>().Subject;

        dto.ParcelTransportCategoryId.Should().Be(ServiceDataConstants.ParcelTransportCategoryId);
        dto.DefaultLocale.Should().Be("fr");
        dto.SupportedLocales.Should().Contain(new[] { "fr", "en", "ru" });
        dto.ActiveModules.Should().ContainKey("wallet").WhoseValue.Should().BeTrue();
        dto.ActiveModules.Should().ContainKey("marketplace").WhoseValue.Should().BeTrue();
        dto.ActiveModules.Should().ContainKey("documents").WhoseValue.Should().BeTrue();
        dto.ActiveModules.Should().ContainKey("chat").WhoseValue.Should().BeTrue();
    }

    [Fact]
    public async Task GetMobileConfig_WhenAuthenticatedWithoutProfileTypeId_ShouldResolveFromUserProfile()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var configRepo = Substitute.For<IMobileConfigRepository>();
        var profileTypeRepo = Substitute.For<IProfileTypeRepository>();
        var profileRepo = Substitute.For<IProfileRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var logger = Substitute.For<ILogger<MobileConfigController>>();

        var userId = Guid.NewGuid();
        var expectedProfileTypeId = Guid.NewGuid();
        currentUserService.UserId.Returns(userId.ToString());

        var profile = Profile.Domain.Entities.Profile.Create(
            userId,
            expectedProfileTypeId,
            Shared.Domain.ValueObjects.PhoneNumber.Create("+22890000000").Value,
            "John",
            "Doe",
            new System.DateOnly(1990, 1, 1));
        profile.Value.Activate();

        profileRepo.GetByUserIdAsync(userId, Arg.Any<System.Threading.CancellationToken>())
            .Returns(new List<Profile.Domain.Entities.Profile> { profile.Value });

        var specificConfig = new Profile.Domain.Entities.ProfileTypeMobileConfig(
            Guid.NewGuid(),
            expectedProfileTypeId,
            "TG",
            new Dictionary<string, bool> { ["wallet"] = true, ["marketplace"] = false },
            "2.0.0",
            false,
            false,
            null,
            "fr",
            new List<string> { "fr" },
            "test"
        );

        configRepo.GetByProfileTypeIdAsync(expectedProfileTypeId, Arg.Any<System.Threading.CancellationToken>())
            .Returns(specificConfig);

        var controller = new MobileConfigController(configRepo, profileTypeRepo, profileRepo, configuration, currentUserService, logger);

        // Act
        var result = await controller.GetMobileConfig();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<MobileBootstrapConfigDto>().Subject;

        dto.ProfileTypeId.Should().Be(expectedProfileTypeId);
        dto.CountryCode.Should().Be("TG");
        dto.ActiveModules["marketplace"].Should().BeFalse();
    }
}
