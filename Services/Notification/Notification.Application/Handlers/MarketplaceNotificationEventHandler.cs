using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Handlers;

public class MarketplaceNotificationEventHandler(
    ILogger<MarketplaceNotificationEventHandler> logger,
    INotificationService notificationService)
{
    public Task Handle(ServiceListingApprovedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(ServiceListingRejectedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(ServiceRequestCreatedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(ServiceRequestAcceptedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(ServiceRequestCompletedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);
}