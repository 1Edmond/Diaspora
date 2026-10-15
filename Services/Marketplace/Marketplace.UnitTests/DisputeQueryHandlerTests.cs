using FluentAssertions;
using Marketplace.Application.Commands;
using Marketplace.Application.DTOs;
using Marketplace.Application.Handlers.Commands;
using Marketplace.Application.Handlers.Queries;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using NSubstitute;
using Shared.Application.Models;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Wolverine;
using Xunit;

namespace Marketplace.UnitTests;

public class DisputeQueryHandlerTests
{
    private readonly IDisputeRepository _disputeRepository = Substitute.For<IDisputeRepository>();
    private readonly IProfileDirectoryRepository _profileDirectory = Substitute.For<IProfileDirectoryRepository>();
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly GetDisputesQueryHandler _getDisputesHandler;
    private readonly GetDisputeByIdQueryHandler _getDisputeByIdHandler;
    private readonly ResolveDisputeCommandHandler _resolveHandler;

    public DisputeQueryHandlerTests()
    {
        _getDisputesHandler = new GetDisputesQueryHandler(_disputeRepository, _profileDirectory);
        _getDisputeByIdHandler = new GetDisputeByIdQueryHandler(_disputeRepository, _profileDirectory);
        _resolveHandler = new ResolveDisputeCommandHandler(
            _disputeRepository,
            _requestRepository,
            _messageBus,
            _unitOfWork);
    }

    private static void SetProperty<T, TValue>(T obj, string propertyName, TValue value)
    {
        var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }

    [Fact]
    public async Task GetDisputes_ShouldReturnPagedDisputes_WithEnrichedProfileNames()
    {
        // Arrange
        var initiatorId = Guid.NewGuid();
        var defendantId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            Guid.NewGuid(), initiatorId, defendantId, "Livraison colis", 25000m, "XOF", 2.5m);

        var dispute = Dispute.Create(
            requestId, initiatorId, defendantId, "Retard", "Le colis n'est pas arrivé", new[] { "https://cdn.example.com/proof1.jpg" });
        dispute.CreatedAt = DateTime.UtcNow;
        SetProperty(dispute, nameof(Dispute.ServiceRequest), request);

        var pagedList = new PagedList<Dispute>(new List<Dispute> { dispute }, 1, 1, 20);

        _disputeRepository.GetPagedDisputesAsync(DisputeStatus.Opened, 1, 20, Arg.Any<CancellationToken>())
            .Returns(pagedList);

        var initiatorProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), initiatorId);
        initiatorProfile.ApplyActivation("Alice Dupont", "FR", "EUR");

        var defendantProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), defendantId);
        defendantProfile.ApplyActivation("Bob Transport", "SN", "XOF");

        var profilesDict = new Dictionary<Guid, ProfileDirectoryEntry>
        {
            [initiatorId] = initiatorProfile,
            [defendantId] = defendantProfile
        };

        _profileDirectory.GetByUserIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(profilesDict);

        var query = new GetDisputesQuery(DisputeStatus.Opened, 1, 20);

        // Act
        var result = await _getDisputesHandler.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(1);

        var dto = result.Value.Items[0];
        dto.Id.Should().Be(dispute.Id);
        dto.InitiatorId.Should().Be(initiatorId);
        dto.InitiatorName.Should().Be("Alice Dupont");
        dto.DefendantId.Should().Be(defendantId);
        dto.DefendantName.Should().Be("Bob Transport");
        dto.Reason.Should().Be("Retard");
        dto.EvidenceUrls.Should().Contain("https://cdn.example.com/proof1.jpg");
        dto.ServiceRequestAgreedPrice.Should().Be(25000m);
        dto.ServiceRequestCurrency.Should().Be("XOF");
    }

    [Fact]
    public async Task GetDisputes_WhenEmpty_ShouldReturnEmptyPagedList()
    {
        // Arrange
        var emptyPagedList = new PagedList<Dispute>(new List<Dispute>(), 0, 1, 20);
        _disputeRepository.GetPagedDisputesAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(emptyPagedList);

        _profileDirectory.GetByUserIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, ProfileDirectoryEntry>());

        var query = new GetDisputesQuery(null, 1, 20);

        // Act
        var result = await _getDisputesHandler.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDisputeById_WhenExists_ShouldReturnEnrichedDto()
    {
        // Arrange
        var disputeId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var defendantId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            Guid.NewGuid(), initiatorId, defendantId, "Achat matériel", 50000m, "EUR");
        var dispute = Dispute.Create(requestId, initiatorId, defendantId, "Article défectueux", "Produit cassé à la livraison");
        dispute.CreatedAt = DateTime.UtcNow;
        SetProperty(dispute, nameof(Dispute.ServiceRequest), request);

        _disputeRepository.GetByIdWithDetailsAsync(disputeId, Arg.Any<CancellationToken>())
            .Returns(dispute);

        var initiatorProfile = ProfileDirectoryEntry.Create(Guid.NewGuid(), initiatorId);
        initiatorProfile.ApplyActivation("Charlie", "FR", "EUR");

        var profilesDict = new Dictionary<Guid, ProfileDirectoryEntry>
        {
            [initiatorId] = initiatorProfile
        };

        _profileDirectory.GetByUserIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(profilesDict);

        var query = new GetDisputeByIdQuery(disputeId);

        // Act
        var result = await _getDisputeByIdHandler.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Reason.Should().Be("Article défectueux");
        result.Value.InitiatorName.Should().Be("Charlie");
        result.Value.DefendantName.Should().BeNull();
        result.Value.ServiceRequestAgreedPrice.Should().Be(50000m);
        result.Value.ServiceRequestCurrency.Should().Be("EUR");
    }

    [Fact]
    public async Task GetDisputeById_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var disputeId = Guid.NewGuid();
        _disputeRepository.GetByIdWithDetailsAsync(disputeId, Arg.Any<CancellationToken>())
            .Returns((Dispute?)null);

        var query = new GetDisputeByIdQuery(disputeId);

        // Act
        var result = await _getDisputeByIdHandler.Handle(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Litige introuvable");
    }

    [Fact]
    public async Task ResolveDispute_SplitResolution_ShouldArbitrateWithPercentages()
    {
        // Arrange
        var initiatorId = Guid.NewGuid();
        var defendantId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var request = ServiceRequest.Create(
            Guid.NewGuid(), initiatorId, defendantId, "Transport fragile", 30000m, "XOF");
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid());

        var dispute = Dispute.Create(request.Id, initiatorId, defendantId, "Délai dépassé", "Marchandise partiellement endommagée");
        request.OpenDispute(dispute.Id);

        _disputeRepository.GetByIdAsync(dispute.Id).Returns(dispute);
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new ResolveDisputeCommand(
            dispute.Id,
            adminId,
            DisputeStatus.ResolvedSplit,
            "Arbitrage 50/50 convenu suite à l'accord des deux parties.",
            BuyerRefundPercentage: 50m,
            SellerPayoutPercentage: 50m);

        // Act
        var result = await _resolveHandler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.IsInDispute.Should().BeFalse();
        dispute.Status.Should().Be(DisputeStatus.ResolvedSplit);
        dispute.ResolvedBy.Should().Be(adminId);
        dispute.ResolutionNotes.Should().Contain("Arbitrage 50/50");
        dispute.BuyerRefundPercentage.Should().Be(50m);
        dispute.SellerPayoutPercentage.Should().Be(50m);

        await _disputeRepository.Received(1).UpdateAsync(dispute);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
