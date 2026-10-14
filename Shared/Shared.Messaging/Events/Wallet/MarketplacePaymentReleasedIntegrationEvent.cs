using System;

namespace Shared.Messaging.Events.Wallet;

public sealed record MarketplacePaymentReleasedIntegrationEvent(
    Guid ServiceRequestId,
    Guid TransactionId
) : IntegrationEvent;
