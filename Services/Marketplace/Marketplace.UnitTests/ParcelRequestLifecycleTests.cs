using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;
using Xunit;

namespace Marketplace.UnitTests;

public class ParcelRequestLifecycleTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly ILogger<CreateServiceRequestCommandHandler> _createLogger = Substitute.For<ILogger<CreateServiceRequestCommandHandler>>();

    [Fact]
    public void OtpSecret_VerificationAndLockout_ShouldLockAfterFiveFailedAttempts()
    {
        var secret = OtpSecret.Generate(out var rawCode);

        // Code must be 6 digits
        rawCode.Should().HaveLength(6);
        rawCode.All(char.IsDigit).Should().BeTrue();

        // 4 failed attempts
        for (int i = 1; i <= 4; i++)
        {
            var verified = secret.Verify("000000");
            verified.Should().BeFalse();
            secret.FailedAttempts.Should().Be(i);
            secret.IsLockedOut.Should().BeFalse();
        }

        // 5th failed attempt locks out
        var fifthAttempt = secret.Verify("000000");
        fifthAttempt.Should().BeFalse();
        secret.FailedAttempts.Should().Be(5);
        secret.IsLockedOut.Should().BeTrue();

        // Even with the correct code now, it must fail because it's locked out
        var correctAttemptWhenLocked = secret.Verify(rawCode);
        correctAttemptWhenLocked.Should().BeFalse();
    }

    [Fact]
    public void OtpSecret_SuccessfulVerification_ShouldReturnTrue()
    {
        var secret = OtpSecret.Generate(out var rawCode);

        var verified = secret.Verify(rawCode);
        verified.Should().BeTrue();
        secret.FailedAttempts.Should().Be(0);
        secret.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void ParcelRequest_PhysicalLifecycle_ShouldTransitionThroughAllSteps()
    {
        var listingId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            listingId,
            requesterId,
            providerId,
            message: "Envoi de 5kg de documents et habits",
            agreedPrice: 5000m,
            agreedCurrency: "RUB",
            weightKg: 5m,
            packageDescription: "Documents et vêtements",
            packageContentCategory: "Textile",
            declaredValue: 15000m);

        request.WeightKg.Should().Be(5m);
        request.PackageDescription.Should().Be("Documents et vêtements");
        request.Status.Should().Be(ServiceRequestStatus.Pending);

        // 1. Accept
        request.Accept();
        request.Status.Should().Be(ServiceRequestStatus.Accepted);

        // 2. MarkPaymentHeld generates OTPs
        request.GenerateOtps(out var handoverOtp, out var deliveryOtp);
        request.MarkPaymentHeld(Guid.NewGuid());
        request.HandoverOtpHash.Should().NotBeNullOrEmpty();
        request.DeliveryOtpHash.Should().NotBeNullOrEmpty();

        // 3. Confirm Handover with wrong OTP fails
        var failHandover = request.ConfirmHandover("111111", null);
        failHandover.IsFailure.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Accepted);

        // 4. Confirm Handover with valid OTP
        var okHandover = request.ConfirmHandover(handoverOtp, "https://storage/proof-handover.jpg");
        okHandover.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.PackageHandedOver);
        request.HandoverConfirmedAt.Should().NotBeNull();
        request.HandoverProofUrl.Should().Be("https://storage/proof-handover.jpg");

        // 5. In transit
        var inTransit = request.MarkInTransit();
        inTransit.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.InTransit);

        // 6. Arrived at destination
        var arrived = request.MarkArrivedAtDestination();
        arrived.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.ArrivedAtDestination);

        // 7. Confirm Delivery with valid OTP
        var okDelivery = request.ConfirmDelivery(deliveryOtp, "https://storage/proof-delivery.jpg");
        okDelivery.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Delivered);
        request.DeliveryConfirmedAt.Should().NotBeNull();
        request.DeliveryProofUrl.Should().Be("https://storage/proof-delivery.jpg");
        request.CompletedAt.Should().NotBeNull();

        // Should publish ServiceRequestCompletedEvent
        var completedEvent = request.DomainEvents.OfType<ServiceRequestCompletedEvent>().FirstOrDefault();
        completedEvent.Should().NotBeNull();
        completedEvent!.RequestId.Should().Be(request.Id);
    }

    [Fact]
    public async Task AcceptServiceRequestCommandHandler_WithParcel_ShouldReserveListingCapacity()
    {
        var handler = new AcceptServiceRequestCommandHandler(
            _requestRepository, _listingRepository, _unitOfWork);

        var providerId = Guid.NewGuid();
        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Vol Lomé-Moscou", "Desc", null, PaymentMode.WalletPayment, null, "RUB");
        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 20m, pricePerKg: 1000m);

        var request = ServiceRequest.Create(
            listing.Id, Guid.NewGuid(), providerId, "5kg colis", 5000m, "RUB",
            weightKg: 5m);

        _requestRepository.GetByIdAsync(request.Id).Returns(request);
        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);

        var result = await handler.Handle(new AcceptServiceRequestCommand(request.Id, providerId));

        result.IsSuccess.Should().BeTrue();
        listing.RemainingKg.Should().Be(15m);
        request.Status.Should().Be(ServiceRequestStatus.Accepted);
    }

    [Fact]
    public async Task AcceptServiceRequestCommandHandler_WhenCapacityInsufficient_ShouldFailAndNotAccept()
    {
        var handler = new AcceptServiceRequestCommandHandler(
            _requestRepository, _listingRepository, _unitOfWork);

        var providerId = Guid.NewGuid();
        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Vol Lomé-Moscou", "Desc", null, PaymentMode.WalletPayment, null, "RUB");
        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 10m, pricePerKg: 1000m);
        listing.ReserveCapacity(8m); // Remaining = 2kg

        var request = ServiceRequest.Create(
            listing.Id, Guid.NewGuid(), providerId, "5kg colis", 5000m, "RUB",
            weightKg: 5m);

        _requestRepository.GetByIdAsync(request.Id).Returns(request);
        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);

        var result = await handler.Handle(new AcceptServiceRequestCommand(request.Id, providerId));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("insuffisante");
        listing.RemainingKg.Should().Be(2m);
        request.Status.Should().Be(ServiceRequestStatus.Pending);
    }

    [Fact]
    public async Task CancelServiceRequestCommandHandler_WhenAccepted_ShouldReleaseListingCapacity()
    {
        var handler = new CancelServiceRequestCommandHandler(
            _requestRepository, _unitOfWork, _messageBus, _listingRepository);

        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Vol Lomé-Moscou", "Desc", null, PaymentMode.WalletPayment, null, "RUB");
        listing.SetTravelDetails(
            "TG", "Lomé", "RU", "Moscou",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null,
            capacityKg: 20m, pricePerKg: 1000m);

        listing.ReserveCapacity(5m); // Remaining = 15m

        var request = ServiceRequest.Create(
            listing.Id, requesterId, providerId, "5kg colis", 5000m, "RUB",
            weightKg: 5m);
        request.Accept();

        _requestRepository.GetByIdAsync(request.Id).Returns(request);
        _listingRepository.GetByIdAsync(listing.Id).Returns(listing);

        var result = await handler.Handle(new CancelServiceRequestCommand(request.Id, requesterId, "Voyage annulé"));

        result.IsSuccess.Should().BeTrue();
        listing.RemainingKg.Should().Be(20m);
        request.Status.Should().Be(ServiceRequestStatus.Cancelled);
    }

    [Fact]
    public async Task ConfirmDeliveryCommandHandler_WithValidOtp_ShouldCompleteAndPublishReleaseRequested()
    {
        var handler = new ConfirmDeliveryCommandHandler(
            _requestRepository, _unitOfWork, _messageBus);

        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            Guid.NewGuid(), requesterId, providerId, "5kg colis", 5000m, "RUB", weightKg: 5m);
        request.Accept();
        request.GenerateOtps(out var handoverOtp, out var deliveryOtp);
        request.MarkPaymentHeld(Guid.NewGuid());
        request.ConfirmHandover(handoverOtp, null);
        request.MarkInTransit();
        request.MarkArrivedAtDestination();

        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new ConfirmDeliveryCommand(request.Id, requesterId, deliveryOtp, "https://proof.jpg");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Delivered);

        await _messageBus.Received(1).PublishAsync(
            Arg.Is<MarketplacePaymentReleaseRequestedIntegrationEvent>(e => e.ServiceRequestId == request.Id));
    }
}
