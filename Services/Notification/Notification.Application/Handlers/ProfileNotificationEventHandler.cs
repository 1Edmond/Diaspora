using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Handlers;

public class ProfileNotificationEventHandler(
    ILogger<ProfileNotificationEventHandler> logger,
    INotificationService notificationService)
{

    public Task Handle(InternalProfileCreatedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);
}
