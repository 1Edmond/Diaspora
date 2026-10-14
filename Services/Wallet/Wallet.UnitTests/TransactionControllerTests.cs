using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Application.Models;
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

public class TransactionControllerTests
{
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly TransactionController _controller;

    public TransactionControllerTests()
    {
        _controller = new TransactionController(_messageBus, _walletRepository, _currentUserService);
    }

    [Fact]
    public async Task GetTransactions_WhenNotOwnerAndNotAdmin_ShouldReturn403()
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
        var result = await _controller.GetTransactions(profileId, 1, 20, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<PagedList<TransactionDto>>>(
            Arg.Any<GetTransactionsQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTransactions_WhenOwner_ShouldReturn200()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsInRole("Admin").Returns(false);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var pagedList = new PagedList<TransactionDto>(new List<TransactionDto>(), 0, 1, 20);
        _messageBus.InvokeAsync<Result<PagedList<TransactionDto>>>(
            Arg.Any<GetTransactionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(pagedList));

        // Act
        var result = await _controller.GetTransactions(profileId, 1, 20, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetTransactions_WhenAdmin_ShouldReturn200EvenIfNotOwner()
    {
        // Arrange
        var profileId = Guid.NewGuid();

        _currentUserService.IsInRole("Admin").Returns(true);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(Guid.NewGuid().ToString());

        var pagedList = new PagedList<TransactionDto>(new List<TransactionDto>(), 0, 1, 20);
        _messageBus.InvokeAsync<Result<PagedList<TransactionDto>>>(
            Arg.Any<GetTransactionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(pagedList));

        // Act
        var result = await _controller.GetTransactions(profileId, 1, 20, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Transfer_WhenNotOwnerOfSenderWallet_ShouldReturn403()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var senderProfileId = Guid.NewGuid();
        var receiverProfileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(senderProfileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(senderProfileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new TransferRequest(
            senderProfileId,
            receiverProfileId,
            50m,
            "Virement non autorisé");

        // Act
        var result = await _controller.Transfer(request, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<TransferResponse>>(
            Arg.Any<TransferCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Transfer_WhenOwnerOfSenderWallet_ShouldReturn200()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var senderProfileId = Guid.NewGuid();
        var receiverProfileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(senderProfileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(senderProfileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new TransferRequest(
            senderProfileId,
            receiverProfileId,
            50m,
            "Virement légitime");

        var response = new TransferResponse(
            Guid.NewGuid(),
            50m,
            0m,
            100m);

        _messageBus.InvokeAsync<Result<TransferResponse>>(
            Arg.Any<TransferCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        // Act
        var result = await _controller.Transfer(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task PayProcedure_WhenNotOwner_ShouldReturn403()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new ProcedurePaymentRequest(
            profileId,
            Guid.NewGuid(),
            25m,
            "Paiement procédure tiers");

        // Act
        var result = await _controller.PayProcedure(request, CancellationToken.None);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        await _messageBus.DidNotReceive().InvokeAsync<Result<Guid>>(
            Arg.Any<ProcedurePaymentCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PayProcedure_WhenOwner_ShouldReturn200()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>()).Returns(wallet);

        var request = new ProcedurePaymentRequest(
            profileId,
            Guid.NewGuid(),
            25m,
            "Paiement procédure personnelle");

        _messageBus.InvokeAsync<Result<Guid>>(
            Arg.Any<ProcedurePaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(Guid.NewGuid()));

        // Act
        var result = await _controller.PayProcedure(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
