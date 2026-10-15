using FluentAssertions;
using NSubstitute;
using Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Dtos;
using Wallet.Application.Handlers.Queries;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;
using Xunit;

namespace Wallet.UnitTests;

public class AdminWalletQueriesTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();

    [Fact]
    public async Task GetWalletsAdminQueryHandler_WhenWalletsExist_ShouldReturnPagedList()
    {
        // Arrange
        var profileId1 = Guid.NewGuid();
        var profileId2 = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var wallet1 = Wallet.Domain.Entities.Wallet.Create(profileId1, userId1, "EUR").Value;
        wallet1.Deposit(500m, "Initial credit");
        var wallet2 = Wallet.Domain.Entities.Wallet.Create(profileId2, userId2, "USD").Value;

        var pagedWallets = new PagedList<Wallet.Domain.Entities.Wallet>(
            new List<Wallet.Domain.Entities.Wallet> { wallet1, wallet2 },
            count: 2,
            pageNumber: 1,
            pageSize: 10
        );

        _walletRepository.GetWalletsAdminAsync("EUR", true, 1, 10, Arg.Any<CancellationToken>())
            .Returns(pagedWallets);

        var handler = new GetWalletsAdminQueryHandler(_walletRepository);
        var query = new GetWalletsAdminQuery("EUR", true, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.Items.Should().HaveCount(2);

        var first = result.Value.Items[0];
        first.ExternalProfileId.Should().Be(profileId1);
        first.Currency.Should().Be("EUR");
        first.Balance.Should().Be(500m);
    }

    [Fact]
    public async Task GetPlatformRevenueQueryHandler_WhenWalletsAndHeldBalancesExist_ShouldConsolidateCorrectly()
    {
        // Arrange
        var eurPlatform = Wallet.Domain.Entities.Wallet.Create(
            Wallet.Domain.Entities.Wallet.PlatformProfileId,
            Wallet.Domain.Entities.Wallet.PlatformUserId,
            "EUR").Value;
        eurPlatform.Deposit(1250.50m, "Platform commissions EUR");

        var xofPlatform = Wallet.Domain.Entities.Wallet.Create(
            Wallet.Domain.Entities.Wallet.PlatformProfileId,
            Wallet.Domain.Entities.Wallet.PlatformUserId,
            "XOF").Value;
        xofPlatform.Deposit(50000m, "Platform commissions XOF");

        var platformWallets = new List<Wallet.Domain.Entities.Wallet> { eurPlatform, xofPlatform };
        var heldBalances = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["EUR"] = 300m,
            ["USD"] = 100m
        };

        _walletRepository.GetPlatformWalletsAsync(Arg.Any<CancellationToken>())
            .Returns(platformWallets);
        _walletRepository.GetTotalHeldBalanceByCurrencyAsync(Arg.Any<CancellationToken>())
            .Returns(heldBalances);

        var handler = new GetPlatformRevenueQueryHandler(_walletRepository);
        var query = new GetPlatformRevenueQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PlatformWallets.Should().HaveCount(2);
        result.Value.Currencies.Should().HaveCount(3); // EUR, USD, XOF

        var eurSummary = result.Value.Currencies.Find(c => c.Currency == "EUR");
        eurSummary.Should().NotBeNull();
        eurSummary!.PlatformBalance.Should().Be(1250.50m);
        eurSummary.TotalEscrowHeld.Should().Be(300m);

        var xofSummary = result.Value.Currencies.Find(c => c.Currency == "XOF");
        xofSummary.Should().NotBeNull();
        xofSummary!.PlatformBalance.Should().Be(50000m);
        xofSummary.TotalEscrowHeld.Should().Be(0m);

        var usdSummary = result.Value.Currencies.Find(c => c.Currency == "USD");
        usdSummary.Should().NotBeNull();
        usdSummary!.PlatformBalance.Should().Be(0m);
        usdSummary.TotalEscrowHeld.Should().Be(100m);
    }

    [Fact]
    public async Task GetPlatformRevenueQueryHandler_WhenNoData_ShouldReturnEmptySummaries()
    {
        // Arrange
        _walletRepository.GetPlatformWalletsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Wallet.Domain.Entities.Wallet>());
        _walletRepository.GetTotalHeldBalanceByCurrencyAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, decimal>());

        var handler = new GetPlatformRevenueQueryHandler(_walletRepository);

        // Act
        var result = await handler.Handle(new GetPlatformRevenueQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Currencies.Should().BeEmpty();
        result.Value.PlatformWallets.Should().BeEmpty();
    }
}
