using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Models;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Handlers;

public static class BaseNotificationEventHandler
{
    // ✅ static async (pas async static)
    public static async Task HandleAsync<TEvent>(
        TEvent message,
        ILogger logger,
        INotificationService notificationService)
        where TEvent : IIntegrationEvent
    {
        try
        {
            logger.LogInformation(
                "[NotificationService] Handling {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);

            var context = CreateNotificationContext(message);
            await notificationService.EnqueueNotificationAsync(context);

            logger.LogInformation(
                "[NotificationService] Successfully enqueued notification for {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[NotificationService] Error handling {EventType} - EventId: {EventId}",
                typeof(TEvent).Name,
                message.EventId);
            throw;
        }
    }

    private static NotificationContext CreateNotificationContext<TEvent>(TEvent message)
        where TEvent : IIntegrationEvent
    {
        return new NotificationContext
        {
            MessageId = message.EventId,
            EventType = typeof(TEvent).Name,
            Payload = message
        };
    }
}
