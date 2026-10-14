using System;

namespace Shared.Messaging.Events.Wallet;

public sealed record MarketplacePaymentHeldIntegrationEvent(
    Guid ServiceRequestId,
    Guid TransactionId
) : IntegrationEvent;
