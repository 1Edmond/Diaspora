using FluentAssertions;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Wallet.Api.Security;
using Wallet.Application.Interfaces;
using Xunit;

namespace Wallet.UnitTests;

public class WalletOwnershipTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    [Fact]
    public async Task IsOwnerAsync_WhenUserNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(false);
        _currentUser.UserId.Returns((string?)null);

        // Act
        var result = await WalletOwnership.IsOwnerAsync(
            _walletRepository, _currentUser, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOwnerAsync_WhenUserIdIsNotValidGuid_ShouldReturnFalse()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns("not-a-guid");

        // Act
        var result = await WalletOwnership.IsOwnerAsync(
            _walletRepository, _currentUser, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOwnerAsync_WhenWalletNotFound_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(userId.ToString());
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns((Wallet.Domain.Entities.Wallet?)null);

        // Act
        var result = await WalletOwnership.IsOwnerAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOwnerAsync_WhenWalletBelongsToDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await WalletOwnership.IsOwnerAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsOwnerAsync_WhenWalletBelongsToCurrentUser_ShouldReturnTrue()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await WalletOwnership.IsOwnerAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_WhenUserIsAdmin_ShouldReturnTrueEvenIfNotOwner()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _currentUser.IsInRole("Admin").Returns(true);

        // Act
        var result = await WalletOwnership.CanReadAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await _walletRepository.DidNotReceive().GetByExternalProfileIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CanReadAsync_WhenUserIsNotAdminButIsOwner_ShouldReturnTrue()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _currentUser.IsInRole("Admin").Returns(false);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, currentUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await WalletOwnership.CanReadAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanReadAsync_WhenNeitherAdminNorOwner_ShouldReturnFalse()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _currentUser.IsInRole("Admin").Returns(false);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(currentUserId.ToString());

        var wallet = Wallet.Domain.Entities.Wallet.Create(profileId, otherUserId, "EUR").Value;
        _walletRepository.GetByExternalProfileIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Act
        var result = await WalletOwnership.CanReadAsync(
            _walletRepository, _currentUser, profileId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
