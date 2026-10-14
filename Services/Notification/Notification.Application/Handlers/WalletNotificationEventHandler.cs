using Microsoft.Extensions.Logging;
using Notification.Application.Abstractions;
using Shared.Messaging.Events.Wallet;
using System.Threading.Tasks;

namespace Notification.Application.Handlers;

public class WalletNotificationEventHandler(
    ILogger<WalletNotificationEventHandler> logger,
    INotificationService notificationService)
{
    public Task Handle(MarketplacePaymentHeldIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(MarketplacePaymentReleasedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(MarketplacePaymentRefundedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);

    public Task Handle(MarketplacePaymentFailedIntegrationEvent message)
        => BaseNotificationEventHandler.HandleAsync(message, logger, notificationService);
}
