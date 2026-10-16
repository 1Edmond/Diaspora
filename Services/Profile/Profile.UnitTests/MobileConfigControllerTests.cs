using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Profile.Api.Controllers;
using Profile.Application.DTOs;
using Shared.Messaging.Constants;
using System.Collections.Generic;
using Xunit;

namespace Profile.UnitTests;

public class MobileConfigControllerTests
{
    [Fact]
    public void GetMobileConfig_ShouldReturnConfigWithParcelTransportIdAndActiveModules()
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

        var logger = Substitute.For<ILogger<MobileConfigController>>();
        var controller = new MobileConfigController(configuration, logger);

        // Act
        var result = controller.GetMobileConfig();

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
}
