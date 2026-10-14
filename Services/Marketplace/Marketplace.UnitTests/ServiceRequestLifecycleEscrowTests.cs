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

public class ServiceRequestLifecycleEscrowTests
{
    private readonly IServiceRequestRepository _requestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMessageBus _messageBus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Complete_WhenPaymentStatusIsHeld_ShouldPublishReleaseRequested()
    {
        // Arrange
        var handler = new CompleteServiceRequestCommandHandler(_requestRepository, _unitOfWork, _messageBus);
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var request = ServiceRequest.Create(Guid.NewGuid(), requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid());
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new CompleteServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Completed);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentReleaseRequestedIntegrationEvent>(e =>
            e.ServiceRequestId == request.Id));
    }

    [Fact]
    public async Task Complete_WhenPaymentStatusIsNotHeld_ShouldNotPublishReleaseRequested()
    {
        // Arrange
        var handler = new CompleteServiceRequestCommandHandler(_requestRepository, _unitOfWork, _messageBus);
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var request = ServiceRequest.Create(Guid.NewGuid(), requesterId, providerId, "Msg", null, null);
        request.Accept();
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new CompleteServiceRequestCommand(request.Id, requesterId);

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<MarketplacePaymentReleaseRequestedIntegrationEvent>());
    }

    [Fact]
    public async Task Cancel_WhenPaymentStatusIsHeld_ShouldPublishRefundRequested()
    {
        // Arrange
        var handler = new CancelServiceRequestCommandHandler(_requestRepository, _unitOfWork, _messageBus);
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var request = ServiceRequest.Create(Guid.NewGuid(), requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid());
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new CancelServiceRequestCommand(request.Id, requesterId, "Changed my mind");

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Cancelled);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentRefundRequestedIntegrationEvent>(e =>
            e.ServiceRequestId == request.Id));
    }

    [Fact]
    public async Task Decline_WhenPaymentStatusIsHeld_ShouldPublishRefundRequested()
    {
        // Arrange
        var handler = new DeclineServiceRequestCommandHandler(_requestRepository, _unitOfWork, _messageBus);
        var providerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var request = ServiceRequest.Create(Guid.NewGuid(), requesterId, providerId, "Msg", 100m, "EUR");
        request.Accept();
        request.MarkPaymentHeld(Guid.NewGuid());
        _requestRepository.GetByIdAsync(request.Id).Returns(request);

        var command = new DeclineServiceRequestCommand(request.Id, providerId, "Cannot fulfill anymore");

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        request.Status.Should().Be(ServiceRequestStatus.Declined);
        await _requestRepository.Received(1).UpdateAsync(request);
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _messageBus.Received(1).PublishAsync(Arg.Is<MarketplacePaymentRefundRequestedIntegrationEvent>(e =>
            e.ServiceRequestId == request.Id));
    }
}
