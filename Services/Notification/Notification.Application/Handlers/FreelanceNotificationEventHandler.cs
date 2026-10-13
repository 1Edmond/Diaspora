using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Shared.Messaging.Events.Freelance;
using System;

namespace Notification.Application.Handlers;

public class FreelanceNotificationEventHandler(
    ILogger<FreelanceNotificationEventHandler> logger,
    INotificationService notificationService)
{
    public Task Handle(JobPostingPublishedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(JobApplicationCreatedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(JobApplicationAcceptedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(JobApplicationRejectedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(JobApplicationCompletedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(JobMatchFoundIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);
}