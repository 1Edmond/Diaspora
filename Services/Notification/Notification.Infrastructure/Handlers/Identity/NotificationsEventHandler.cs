using Microsoft.Build.Tasks;
using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Notification.Application.Dispatcher;
using Notification.Application.Models;
using Notification.Infrastructure.Services;
using Shared.Application.Interfaces;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Handlers.Identity;

public class NotificationsEventHandler
{
    public static async Task Handle(
     UserCreatedIntegrationEvent message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService) => 
        await HandleNotification(new NotificationContext()
        {
            MessageId = message.EventId,
            EventType = nameof(UserCreatedIntegrationEvent),
            Payload = message
        } , logger, notificationService);
    public static async Task Handle(
     PasswordChangedIntegrationEvent message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService) => 
        await HandleNotification(new NotificationContext()
        {
            MessageId = message.EventId,
            EventType = nameof(PasswordChangedIntegrationEvent),
            Payload = message
        } , logger, notificationService);
    public static async Task Handle(
     UserLockedOutIntegrationEvent message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService) => 
        await HandleNotification(new NotificationContext()
        {
            MessageId = message.EventId,
            EventType = nameof(UserLockedOutIntegrationEvent),
            Payload = message
        } , logger, notificationService);
    public static async Task Handle(
     UserSuspendedIntegrationEvent message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService) => 
        await HandleNotification(new NotificationContext()
        {
            MessageId = message.EventId,
            EventType = nameof(UserSuspendedIntegrationEvent),
            Payload = message
        } , logger, notificationService);

    
    public static async Task Handle(
     EmailVerifiedIntegrationEvent message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService) => 
        await HandleNotification(new NotificationContext()
        {
            MessageId = message.EventId,
            EventType = nameof(EmailVerifiedIntegrationEvent),
            Payload = message
        } , logger, notificationService);


    public static async Task Handle(
    UserLoggedInIntegrationEvent message,
    ILogger<NotificationsEventHandler> logger,
    INotificationService notificationService) =>
       await HandleNotification(new NotificationContext()
       {
           MessageId = message.EventId,
           EventType = nameof(UserLoggedInIntegrationEvent),
           Payload = message
       }, logger, notificationService);




    private static async Task HandleNotification(NotificationContext message,
     ILogger<NotificationsEventHandler> logger,
     INotificationService notificationService)
    {
        try
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Handling Notification Id: {MessageId}", message.MessageId);

            await notificationService.EnqueueNotificationAsync(message);

        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "Error handling Notification for Id: {MessageId}", message.MessageId);
        }
    }
}
