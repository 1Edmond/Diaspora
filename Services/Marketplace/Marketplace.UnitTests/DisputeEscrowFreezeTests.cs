using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Threading.Tasks;
using Wolverine;
using Xunit;

namespace Marketplace.UnitTests;

public class DisputeEscrowFreezeTests
{
    private readonly IDisputeRepository _disputeRepository = Substitute.For<IDisputeRepository>();
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly OpenDisputeCommandHandler _openHandler;
    private readonly ResolveDisputeCommandHandler _resolveHandler;

    public DisputeEscrowFreezeTests()
    {
        _openHandler = new OpenDisputeCommandHandler(
            _requestRepository,
            _disputeRepository,
            _unitOfWork);

        _resolveHandler = new ResolveDisputeCommandHandler(
            _disputeRepository,
            _requestRepository,
            _messageBus,
            _unitOfWork);
    }

    private static ServiceRequest CreateAcceptedHeldRequest(Guid requesterId, Guid providerId)
    {
        var request = ServiceRequest.Create(
            Guid.NewGuid(), requesterId, providerId, "Demande colis", 15000m, "XOF", 5.0m);
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid());
        return request;
    }

    [Fact]
    public async Task OpenDispute_ByRequester_ShouldSucceedAndFreezeRequest()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);

        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new OpenDisputeCommand(
            request.Id,
            requesterId,
            "Colis endommagé",
            "Le colis a été ouvert et des objets manquent.");

        // Act
        var result = await _openHandler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        request.IsInDispute.Should().BeTrue();
        request.ActiveDisputeId.Should().Be(result.Value);

        await _disputeRepository.Received(1).AddAsync(Arg.Is<Dispute>(d =>
            d.ServiceRequestId == request.Id &&
            d.InitiatorId == requesterId &&
            d.DefendantId == providerId &&
            d.Reason == "Colis endommagé" &&
            d.Status == DisputeStatus.Opened));

        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task OpenDispute_ByNonStakeholder_ShouldFail()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);

        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new OpenDisputeCommand(
            request.Id,
            strangerId,
            "Raison quelconque",
            "Description");

        // Act
        var result = await _openHandler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("parties prenantes");
        request.IsInDispute.Should().BeFalse();
        await _disputeRepository.DidNotReceive().AddAsync(Arg.Any<Dispute>());
    }

    [Fact]
    public async Task OpenDispute_WhenAlreadyInDispute_ShouldFail()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);
        request.OpenDispute(Guid.NewGuid());

        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new OpenDisputeCommand(
            request.Id,
            providerId,
            "Autre litige",
            "Description");

        // Act
        var result = await _openHandler.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("déjà ouvert");
    }

    [Fact]
    public void ConfirmDelivery_WhenRequestIsInDispute_ShouldBeBlocked()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);
        request.OpenDispute(Guid.NewGuid());

        // Act
        var result = request.ConfirmDelivery("123456", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("litige est actuellement en cours");
    }

    [Fact]
    public void ConfirmHandover_WhenRequestIsInDispute_ShouldBeBlocked()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);
        request.OpenDispute(Guid.NewGuid());

        // Act
        var result = request.ConfirmHandover("123456", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("litige est actuellement en cours");
    }

    [Fact]
    public async Task ResolveDispute_BuyerRefunded_ShouldUnfreezeAndPublishRefundRequested()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var request = CreateAcceptedHeldRequest(requesterId, providerId);

        var dispute = Dispute.Create(request.Id, requesterId, providerId, "Colis perdu", "Perdu en vol");
        request.OpenDispute(dispute.Id);

        _disputeRepository.GetByIdAsync(dispute.Id).Returns(dispute);
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new ResolveDisputeCommand(
            dispute.Id,
            adminId,
            DisputeStatus.ResolvedBuyerRefunded,
            "Perte confirmée par le transporteur. Remboursement intégral de l'acheteur.",
            BuyerRefundPercentage: 100m,
            SellerPayoutPercentage: 0m);

        // Act
        var result = await _resolveHandler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.IsInDispute.Should().BeFalse();
        request.ActiveDisputeId.Should().BeNull();

        dispute.Status.Should().Be(DisputeStatus.ResolvedBuyerRefunded);
        dispute.ResolvedBy.Should().Be(adminId);
        dispute.BuyerRefundPercentage.Should().Be(100m);

        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentRefundRequestedIntegrationEvent>(e =>
            e.ServiceRequestId == request.Id));

        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
