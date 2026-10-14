using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Wallet.Api.Controllers;
using Wallet.Application.Commands;
using Wallet.Application.Dtos;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;
using Wallet.Application.Requests;
using Wallet.Application.Responses;
using Wolverine;
using Xunit;

namespace Wallet.UnitTests;

public class WalletControllerTests
{
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly WalletController _controller;

    public WalletControllerTests()
    {
        _controller = new WalletController(_messageBus, _walletRepository, _currentUserService);
    }

    [Fact]
    public async Task GetByProfile_WhenUserIsNotOwnerAndNotAdmin_ShouldReturn403()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsInRole("Admin").Returns(false);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        // Act
        var result = await _controller.GetByProfile(profileId, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<WalletDto>>(
            Arg.Any<GetWalletByProfileQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByProfile_WhenUserIsOwner_ShouldReturn200()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsInRole("Admin").Returns(false);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var walletDto = new WalletDto(
            wallet.Id,
            profileId,
            currentUserId,
            "EUR",
            150m,
            true,
            DateTime.UtcNow);

        _messageBus.InvokeAsync<Result<WalletDto>>(
            Arg.Any<GetWalletByProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(walletDto));

        // Act
        var result = await _controller.GetByProfile(profileId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(walletDto);
    }

    [Fact]
    public async Task GetByProfile_WhenUserIsAdmin_ShouldReturn200EvenIfNotOwner()
    {
        // Arrange
        var profileId = Guid.NewGuid();

        _currentUserService.IsInRole("Admin").Returns(true);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(Guid.NewGuid().ToString());

        var walletDto = new WalletDto(
            Guid.NewGuid(),
            profileId,
            Guid.NewGuid(),
            "EUR",
            500m,
            true,
            DateTime.UtcNow);

        _messageBus.InvokeAsync<Result<WalletDto>>(
            Arg.Any<GetWalletByProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(walletDto));

        // Act
        var result = await _controller.GetByProfile(profileId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Deposit_WhenUserIsNotAdmin_ShouldReturn403DefenseInDepth()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _currentUserService.IsInRole("Admin").Returns(false);

        var request = new DepositRequest(100m, "Dépôt d'argent");

        // Act
        var result = await _controller.Deposit(profileId, request, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<DepositResponse>>(
            Arg.Any<DepositCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deposit_WhenUserIsAdmin_ShouldReturn200()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _currentUserService.IsInRole("Admin").Returns(true);

        var request = new DepositRequest(100m, "Dépôt validé par admin");
        var depositResponse = new DepositResponse
        {
            Id = Guid.NewGuid(),
            WalletId = Guid.NewGuid(),
            Amount = 100m,
            Fee = 0m,
            BalanceAfter = 100m,
            Currency = "EUR"
        };

        _messageBus.InvokeAsync<Result<DepositResponse>>(
            Arg.Any<DepositCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(depositResponse));

        // Act
        var result = await _controller.Deposit(profileId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(depositResponse);
    }

    [Fact]
    public async Task Withdraw_WhenUserIsNotOwner_ShouldReturn403()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new WithdrawRequest(50m, "Tentative de retrait");

        // Act
        var result = await _controller.Withdraw(profileId, request, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<WithdrawResponse>>(
            Arg.Any<WithdrawCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Withdraw_WhenUserIsOwner_ShouldReturn200()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new WithdrawRequest(50m, "Retrait légitime");
        var response = new WithdrawResponse
        {
            TransactionId = Guid.NewGuid(),
            WalletId = wallet.Id,
            Amount = 50m,
            BalanceAfter = 50m,
            Currency = "EUR",
            Status = "Completed"
        };

        _messageBus.InvokeAsync<Result<WithdrawResponse>>(
            Arg.Any<WithdrawCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var result = await _controller.Withdraw(profileId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
