using FluentAssertions;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Infrastructure.Consumers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class MarketplacePaymentEscrowConsumersTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task PaymentHeldConsumer_WhenValid_ShouldSetStatusToHeld()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentHeldConsumer>>();
        var consumer = new MarketplacePaymentHeldConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        request.MarkPaymentProcessing();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var txId = Guid.NewGuid();
        var @event = new MarketplacePaymentHeldIntegrationEvent(request.Id, txId);

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Held);
        request.WalletTransactionId.Should().Be(txId);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PaymentHeldConsumer_WhenAlreadyHeld_ShouldBeIdempotent()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentHeldConsumer>>();
        var consumer = new MarketplacePaymentHeldConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        var existingTxId = Guid.NewGuid();
        request.MarkPaymentHeld(existingTxId);
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var @event = new MarketplacePaymentHeldIntegrationEvent(request.Id, Guid.NewGuid());

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Held);
        request.WalletTransactionId.Should().Be(existingTxId);
        await _requestRepository.DidNotReceive().UpdateAsync(request);
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task PaymentFailedConsumer_WhenValid_ShouldSetStatusToFailed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentFailedConsumer>>();
        var consumer = new MarketplacePaymentFailedConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        request.MarkPaymentProcessing();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var @event = new MarketplacePaymentFailedIntegrationEvent(request.Id, "Insufficient balance");

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Failed);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PaymentFailedConsumer_WhenAlreadyFailed_ShouldBeIdempotent()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentFailedConsumer>>();
        var consumer = new MarketplacePaymentFailedConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        request.MarkPaymentFailed();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var @event = new MarketplacePaymentFailedIntegrationEvent(request.Id, "Some reason");

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Failed);
        await _requestRepository.DidNotReceive().UpdateAsync(request);
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task PaymentReleasedConsumer_WhenValid_ShouldSetStatusToReleased()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentReleasedConsumer>>();
        var consumer = new MarketplacePaymentReleasedConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        request.MarkPaymentHeld(Guid.NewGuid());
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var txId = Guid.NewGuid();
        var @event = new MarketplacePaymentReleasedIntegrationEvent(request.Id, txId);

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Released);
        request.WalletTransactionId.Should().Be(txId);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PaymentRefundedConsumer_WhenValid_ShouldSetStatusToRefunded()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MarketplacePaymentRefundedConsumer>>();
        var consumer = new MarketplacePaymentRefundedConsumer(_requestRepository, _unitOfWork, logger);

        var request = ServiceRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Msg", 100m, "EUR");
        request.MarkPaymentHeld(Guid.NewGuid());
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var txId = Guid.NewGuid();
        var @event = new MarketplacePaymentRefundedIntegrationEvent(request.Id, txId);

        // Act
        await consumer.Handle(@event);

        // Assert
        request.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        request.WalletTransactionId.Should().Be(txId);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
