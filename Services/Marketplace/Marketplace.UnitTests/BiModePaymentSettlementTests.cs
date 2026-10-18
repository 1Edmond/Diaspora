using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Marketplace.UnitTests;

public class BiModePaymentSettlementTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IServiceListingRepository _listingRepository = Substitute.For<IServiceListingRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IProfileTypePaymentPolicyRepository _paymentPolicyRepository = Substitute.For<IProfileTypePaymentPolicyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateServiceRequestCommandHandler _handler;

    public BiModePaymentSettlementTests()
    {
        _handler = new CreateServiceRequestCommandHandler(
            _requestRepository,
            _listingRepository,
            _categoryRepository,
            _profileDirectory,
            _paymentPolicyRepository,
            _unitOfWork);
    }

    [Fact]
    public void ProfileTypePaymentPolicy_CalculateDirectPlatformFee_CalculatesCorrectly()
    {
        // 1. Fee disabled -> returns 0
        var disabledPolicy = ProfileTypePaymentPolicy.Create(
            Guid.NewGuid(), "TG", AllowedSettlementPolicy.Both, DirectCashTiming.AtPickup,
            directBookingFeeEnabled: false, directBookingFeeFixedAmount: 500m, directBookingFeeRate: 0.05m);
        disabledPolicy.CalculateDirectPlatformFee(10000m).Should().Be(0m);

        // 2. Fee enabled: fixed (200) + rate (5% of 10000 = 500) -> 700
        var enabledPolicy = ProfileTypePaymentPolicy.Create(
            Guid.NewGuid(), "TG", AllowedSettlementPolicy.Both, DirectCashTiming.AtPickup,
            directBookingFeeEnabled: true, directBookingFeeFixedAmount: 200m, directBookingFeeRate: 0.05m);
        enabledPolicy.CalculateDirectPlatformFee(10000m).Should().Be(700m);
    }

    [Fact]
    public void ServiceRequest_ConfirmHandover_SettlesDirectCash_WhenAtPickupOrFreeChoice()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test msg",
            agreedPrice: 5000m, agreedCurrency: "XOF", weightKg: 2m,
            settlementType: PaymentSettlementType.DirectHandover,
            cashTiming: DirectCashTiming.AtPickup,
            platformDirectFeeAmount: 250m);

        request.Accept(); // generates OTPs for direct handover
        request.GenerateOtps(out var rawHandoverOtp, out _);

        request.IsDirectCashSettled.Should().BeFalse();
        request.DirectCashSettledAt.Should().BeNull();

        var result = request.ConfirmHandover(rawHandoverOtp, "http://proof.jpg");
        result.IsSuccess.Should().BeTrue();
        request.IsDirectCashSettled.Should().BeTrue();
        request.DirectCashSettledAt.Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_ConfirmDelivery_SettlesDirectCash_WhenAtDelivery()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test msg",
            agreedPrice: 5000m, agreedCurrency: "XOF", weightKg: 2m,
            settlementType: PaymentSettlementType.DirectHandover,
            cashTiming: DirectCashTiming.AtDelivery,
            platformDirectFeeAmount: 250m);

        request.Accept();
        request.GenerateOtps(out var rawHandoverOtp, out var rawDeliveryOtp);

        request.ConfirmHandover(rawHandoverOtp, "http://proof1.jpg");
        request.IsDirectCashSettled.Should().BeFalse(); // Not settled at pickup because timing is AtDelivery

        request.MarkInTransit();
        var deliveryResult = request.ConfirmDelivery(rawDeliveryOtp, "http://proof2.jpg");
        deliveryResult.IsSuccess.Should().BeTrue();
        request.IsDirectCashSettled.Should().BeTrue();
        request.DirectCashSettledAt.Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_MarkPaymentHeld_WhenDirectHandover_MovesToAcceptedWithoutError()
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Test msg",
            agreedPrice: 5000m, agreedCurrency: "XOF", weightKg: 3m,
            settlementType: PaymentSettlementType.DirectHandover,
            cashTiming: DirectCashTiming.AtPickup);

        request.Status.Should().Be(ServiceRequestStatus.Pending);

        request.MarkPaymentHeld(null);

        request.Status.Should().Be(ServiceRequestStatus.Accepted);
        request.AcceptedAt.Should().NotBeNull();
        request.HandoverOtpHash.Should().NotBeNullOrWhiteSpace();
        request.DeliveryOtpHash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateServiceRequest_WhenSettlementTypeNotAllowedByPolicy_ShouldReturnFailure()
    {
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var requester = ProfileDirectoryEntry.Create(Guid.NewGuid(), requesterId);
        requester.ApplyActivation("Buyer", "TG", "XOF");
        _profileDirectory.GetByUserIdAsync(requesterId).Returns(requester);

        var provider = ProfileDirectoryEntry.Create(Guid.NewGuid(), providerId);
        provider.ApplyActivation("Seller", "TG", "XOF");
        _profileDirectory.GetByUserIdAsync(providerId).Returns(provider);

        var listing = ServiceListing.Create(
            providerId, Guid.NewGuid(), "Title", "Desc", "contact",
            PaymentMode.Both, 1000m, "XOF");
        listing.Approve();
        _listingRepository.GetByIdAsync(listingId).Returns(listing);

        // Policy only allows WalletEscrow
        var escrowOnlyPolicy = ProfileTypePaymentPolicy.Create(
            provider.Id, "TG", AllowedSettlementPolicy.WalletEscrowOnly);
        _paymentPolicyRepository.GetOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(escrowOnlyPolicy);

        var command = new CreateServiceRequestCommand(
            listingId, requesterId, "Test message", 1000m, "XOF",
            SettlementType: PaymentSettlementType.DirectHandover);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Ce mode de règlement n'est pas autorisé pour ce corridor/pays.");
    }
}
