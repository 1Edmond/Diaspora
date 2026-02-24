using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Shared.Messaging.Events.Identity;

namespace Notification.Application.Handlers;

public class IdentityNotificationsEventHandler
{
    private readonly ILogger<IdentityNotificationsEventHandler> _logger;
    private readonly INotificationService _notificationService;

    public IdentityNotificationsEventHandler(
        ILogger<IdentityNotificationsEventHandler> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public Task Handle(UserCreatedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);

    public Task Handle(PasswordChangedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);

    public Task Handle(UserLockedOutIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);

    public Task Handle(UserSuspendedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);

    public Task Handle(EmailVerifiedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);

    public Task Handle(UserLoggedInIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, _logger, _notificationService);
}