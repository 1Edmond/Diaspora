using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Interfaces;
using Wallet.Domain.Entities;
using Wallet.Infrastructure.Consumers;
using Wolverine;
using Xunit;

namespace Wallet.UnitTests;

public class MarketplaceEscrowConsumerTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ILogger<MarketplacePaymentRequestedConsumer> _loggerRequested = Substitute.For<ILogger<MarketplacePaymentRequestedConsumer>>();
    private readonly ILogger<MarketplacePaymentReleaseRequestedConsumer> _loggerRelease = Substitute.For<ILogger<MarketplacePaymentReleaseRequestedConsumer>>();
    private readonly ILogger<MarketplacePaymentRefundRequestedConsumer> _loggerRefund = Substitute.For<ILogger<MarketplacePaymentRefundRequestedConsumer>>();

    [Fact]
    public async Task PaymentRequested_WhenBuyerNotFound_ShouldPublishFailedEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentRequestedConsumer(_walletRepository, _messageBus, _loggerRequested);
        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>())
            .Returns((Domain.Entities.Wallet?)null);

        var @event = new MarketplacePaymentRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId, sellerProfileId, 50m, "EUR");

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentFailedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId && e.Reason.Contains("not found")));
        await _walletRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Domain.Entities.Wallet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PaymentRequested_WhenInsufficientBalance_ShouldPublishFailedEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentRequestedConsumer(_walletRepository, _messageBus, _loggerRequested);
        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyerWallet = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "EUR").Value;
        buyerWallet.Deposit(20m, "Initial");

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>())
            .Returns(buyerWallet);

        var @event = new MarketplacePaymentRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId, sellerProfileId, 50m, "EUR");

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentFailedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId && e.Reason.Contains("Insufficient balance")));
    }

    [Fact]
    public async Task PaymentRequested_WhenCurrencyMismatch_ShouldPublishFailedEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentRequestedConsumer(_walletRepository, _messageBus, _loggerRequested);
        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyerWallet = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "EUR").Value;
        buyerWallet.Deposit(100m, "Initial");

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>())
            .Returns(buyerWallet);

        var @event = new MarketplacePaymentRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId, sellerProfileId, 50m, "USD");

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentFailedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId && e.Reason.Contains("Currency mismatch")));
    }

    [Fact]
    public async Task PaymentRequested_WhenValid_ShouldHoldAndPublishHeldEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentRequestedConsumer(_walletRepository, _messageBus, _loggerRequested);
        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyerWallet = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "EUR").Value;
        buyerWallet.Deposit(100m, "Initial");

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>())
            .Returns(buyerWallet);

        var @event = new MarketplacePaymentRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId, sellerProfileId, 50m, "EUR");

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        buyerWallet.Balance.Should().Be(50m);
        buyerWallet.HeldBalance.Should().Be(50m);
        await _walletRepository.Received(1).UpdateAsync(buyerWallet, Arg.Any<CancellationToken>());
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentHeldIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId));
    }

    [Fact]
    public async Task PaymentReleaseRequested_WhenValid_ShouldReleaseAndPublishReleasedEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentReleaseRequestedConsumer(
            _walletRepository, _transactionRepository, _messageBus, _loggerRelease);

        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();
        var sellerProfileId = Guid.NewGuid();

        var buyer = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "EUR").Value;
        var seller = Domain.Entities.Wallet.Create(sellerProfileId, Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Deposit");
        var holdTx = buyer.HoldForMarketplace(serviceRequestId, 60m, "EUR", sellerProfileId: sellerProfileId).Value;

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>()).Returns(buyer);
        _walletRepository.GetByExternalProfileIdAsync(sellerProfileId, Arg.Any<CancellationToken>()).Returns(seller);
        _transactionRepository.GetHoldTransactionByReferenceIdAsync(serviceRequestId, Arg.Any<CancellationToken>())
            .Returns(holdTx);

        var @event = new MarketplacePaymentReleaseRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId, sellerProfileId);

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        buyer.Balance.Should().Be(40m);
        buyer.HeldBalance.Should().Be(0m);
        seller.Balance.Should().Be(60m);
        await _walletRepository.Received(1).UpdateAsync(buyer, Arg.Any<CancellationToken>());
        await _walletRepository.Received(1).UpdateAsync(seller, Arg.Any<CancellationToken>());
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentReleasedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId));
    }

    [Fact]
    public async Task PaymentRefundRequested_WhenValid_ShouldRefundAndPublishRefundedEvent()
    {
        // Arrange
        var consumer = new MarketplacePaymentRefundRequestedConsumer(
            _walletRepository, _transactionRepository, _messageBus, _loggerRefund);

        var serviceRequestId = Guid.NewGuid();
        var buyerProfileId = Guid.NewGuid();

        var buyer = Domain.Entities.Wallet.Create(buyerProfileId, Guid.NewGuid(), "EUR").Value;
        buyer.Deposit(100m, "Deposit");
        var holdTx = buyer.HoldForMarketplace(serviceRequestId, 60m, "EUR").Value;

        _walletRepository.GetByExternalProfileIdAsync(buyerProfileId, Arg.Any<CancellationToken>()).Returns(buyer);
        _transactionRepository.GetHoldTransactionByReferenceIdAsync(serviceRequestId, Arg.Any<CancellationToken>())
            .Returns(holdTx);

        var @event = new MarketplacePaymentRefundRequestedIntegrationEvent(
            serviceRequestId, buyerProfileId);

        // Act
        await consumer.Handle(@event, CancellationToken.None);

        // Assert
        buyer.Balance.Should().Be(100m);
        buyer.HeldBalance.Should().Be(0m);
        await _walletRepository.Received(1).UpdateAsync(buyer, Arg.Any<CancellationToken>());
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentRefundedIntegrationEvent>(e =>
            e.ServiceRequestId == serviceRequestId));
    }
}
