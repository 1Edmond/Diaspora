using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Notification.Application.Abstractions;
using Notification.Application.Handlers;
using Notification.Application.Models;
using Shared.Messaging.Events.Kyc;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Notification.UnitTests;

public class NotificationHandlersTests
{
    [Fact]
    public async Task MarketplaceNotificationEventHandler_DeclinedAndCancelled_EnqueuesNotification()
    {
        var logger = Substitute.For<ILogger<MarketplaceNotificationEventHandler>>();
        var notificationService = Substitute.For<INotificationService>();
        var handler = new MarketplaceNotificationEventHandler(logger, notificationService);

        var declined = new ServiceRequestDeclinedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Provider unavailable");

        await handler.Handle(declined);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(ServiceRequestDeclinedIntegrationEvent) && c.MessageId == declined.EventId));

        var cancelled = new ServiceRequestCancelledIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "No longer needed");

        await handler.Handle(cancelled);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(ServiceRequestCancelledIntegrationEvent) && c.MessageId == cancelled.EventId));
    }

    [Fact]
    public async Task MarketplaceNotificationEventHandler_ReviewAndReport_EnqueuesNotification()
    {
        var logger = Substitute.For<ILogger<MarketplaceNotificationEventHandler>>();
        var notificationService = Substitute.For<INotificationService>();
        var handler = new MarketplaceNotificationEventHandler(logger, notificationService);

        var reviewCreated = new ReviewCreatedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Great work!");

        await handler.Handle(reviewCreated);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(ReviewCreatedIntegrationEvent)));

        var reviewReplied = new ReviewRepliedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Thank you!");

        await handler.Handle(reviewReplied);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(ReviewRepliedIntegrationEvent)));

        var reportResolved = new ReportResolvedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "Listing", Guid.NewGuid(), "Warning issued");

        await handler.Handle(reportResolved);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(ReportResolvedIntegrationEvent)));
    }

    [Fact]
    public async Task WalletNotificationEventHandler_EscrowEvents_EnqueuesNotification()
    {
        var logger = Substitute.For<ILogger<WalletNotificationEventHandler>>();
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WalletNotificationEventHandler(logger, notificationService);

        var held = new MarketplacePaymentHeldIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());
        await handler.Handle(held);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(MarketplacePaymentHeldIntegrationEvent)));

        var released = new MarketplacePaymentReleasedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());
        await handler.Handle(released);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(MarketplacePaymentReleasedIntegrationEvent)));

        var refunded = new MarketplacePaymentRefundedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());
        await handler.Handle(refunded);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(MarketplacePaymentRefundedIntegrationEvent)));

        var failed = new MarketplacePaymentFailedIntegrationEvent(Guid.NewGuid(), "Insufficient balance");
        await handler.Handle(failed);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(MarketplacePaymentFailedIntegrationEvent)));
    }

    [Fact]
    public async Task KYCNotificationsEventHandler_ArrivalProofApproved_EnqueuesNotification()
    {
        var logger = Substitute.For<ILogger<KYCNotificationsEventHandler>>();
        var notificationService = Substitute.For<INotificationService>();
        var handler = new KYCNotificationsEventHandler(logger, notificationService);

        var arrivalApproved = new KycArrivalProofApprovedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "TG", DateTime.UtcNow);

        await handler.Handle(arrivalApproved);

        await notificationService.Received(1).EnqueueNotificationAsync(
            Arg.Is<NotificationContext>(c => c.EventType == nameof(KycArrivalProofApprovedIntegrationEvent) && c.MessageId == arrivalApproved.EventId));
    }
}
