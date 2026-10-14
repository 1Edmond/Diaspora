using System;

namespace Shared.Messaging.Events.Wallet;

public sealed record MarketplacePaymentFailedIntegrationEvent(
    Guid ServiceRequestId,
    string Reason
) : IntegrationEvent;
