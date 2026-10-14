using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;
using Xunit;

namespace Marketplace.UnitTests;

public class PayServiceRequestCommandHandlerTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IProfileDirectoryRepository _profileDirectoryRepository = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ILogger<PayServiceRequestCommandHandler> _logger = Substitute.For<ILogger<PayServiceRequestCommandHandler>>();

    private readonly PayServiceRequestCommandHandler _handler;

    public PayServiceRequestCommandHandlerTests()
    {
        _handler = new PayServiceRequestCommandHandler(
            _requestRepository,
            _listingRepository,
            _profileDirectoryRepository,
            _unitOfWork,
            _messageBus,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new PayServiceRequestCommand(Guid.NewGuid(), Guid.NewGuid());
        _requestRepository.GetByIdAsync(command.Id).Returns((ServiceRequest?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Request not found.");
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotRequester_ShouldReturnAccessDenied()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 100m, "EUR");
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new PayServiceRequestCommand(request.Id, callerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Access denied.");
    }

    [Fact]
    public async Task Handle_WhenRequestNotAccepted_ShouldReturnFailure()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 100m, "EUR");
        // Status is Pending, not Accepted
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must be accepted");
    }

    [Fact]
    public async Task Handle_WhenListingIsContactOnly_ShouldReturnFailure()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact@test.com",
            PaymentMode.ContactOnly, 100m, "EUR", "City", "Country");
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("does not support platform payment");
    }

    [Fact]
    public async Task Handle_WhenAgreedPriceIsZeroOrMissing_ShouldReturnFailure()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", null, null);
        request.Accept();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact@test.com",
            PaymentMode.WalletPayment, 100m, "EUR", "City", "Country");
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Agreed price must be greater than zero");
    }

    [Fact]
    public async Task Handle_WhenPaymentStatusIsNotPendingOrFailed_ShouldReturnFailure()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid()); // Already held
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact@test.com",
            PaymentMode.WalletPayment, 100m, "EUR", "City", "Country");
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Payment cannot be initiated");
    }

    [Fact]
    public async Task Handle_WhenBuyerProfileNotFoundInDirectory_ShouldReturnFailure()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact@test.com",
            PaymentMode.WalletPayment, 100m, "EUR", "City", "Country");
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        _profileDirectoryRepository.GetByUserIdAsync(requesterId, Arg.Any<CancellationToken>())
            .Returns((ProfileDirectoryEntry?)null);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Buyer profile directory entry not found");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldMarkProcessingAndPublishEvent()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var request = ServiceRequest.Create(listingId, requesterId, providerId, "Msg", 150m, "EUR");
        request.Accept();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact@test.com",
            PaymentMode.WalletPayment, 150m, "EUR", "City", "Country");
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        var buyerProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), requesterId);
        buyerProfile.ApplyActivation("Buyer", "FR", "EUR");
        var sellerProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), providerId);
        sellerProfile.ApplyActivation("Seller", "FR", "EUR");

        _profileDirectoryRepository.GetByUserIdAsync(requesterId, Arg.Any<CancellationToken>())
            .Returns(buyerProfile);
        _profileDirectoryRepository.GetByUserIdAsync(providerId, Arg.Any<CancellationToken>())
            .Returns(sellerProfile);

        var command = new PayServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.PaymentStatus.Should().Be(PaymentStatus.Processing);

        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentRequestedIntegrationEvent>(e =>
            e.ServiceRequestId == request.Id &&
            e.BuyerProfileId == buyerProfile.Id &&
            e.SellerProfileId == sellerProfile.Id &&
            e.Amount == 150m &&
            e.Currency == "EUR"));
    }
}
