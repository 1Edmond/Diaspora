using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Handlers;

public class KYCNotificationsEventHandler
{
    private readonly ILogger<KYCNotificationsEventHandler> _logger;
    private readonly INotificationService _notificationService;

    public KYCNotificationsEventHandler(
        ILogger<KYCNotificationsEventHandler> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public Task Handle(KYCVerificationCreatedIntegrationEvent message)
         => HandleAsync(message);

    public Task Handle(KYCVerificationSubmittedIntegrationEvent message)
        => HandleAsync(message);

    public Task Handle(KYCVerificationApprovedIntegrationEvent message)
        => HandleAsync(message);

    public Task Handle(KYCVerificationRejectedIntegrationEvent message)
        => HandleAsync(message);

    public Task Handle(KYCMoreInfoRequestedIntegrationEvent message)
        => HandleAsync(message);

    public Task Handle(KYCDocumentUploadedIntegrationEvent message)
        => HandleAsync(message);
    private async Task HandleAsync<TEvent>(TEvent message)
        where TEvent : IIntegrationEvent
    {
        try
        {
            _logger.LogInformation(
                "[NotificationService] Handling {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);

            await _notificationService.EnqueueNotificationAsync(new NotificationContext
            {
                MessageId = message.EventId,
                EventType = typeof(TEvent).Name,
                Payload = message
            });

            _logger.LogInformation(
                "[NotificationService] Successfully enqueued notification for {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[NotificationService] Error handling {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);
            throw;
        }
    }
}