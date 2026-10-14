using System;

namespace Shared.Messaging.Events.Wallet;

public sealed record MarketplacePaymentRefundedIntegrationEvent(
    Guid ServiceRequestId,
    Guid TransactionId
) : IntegrationEvent;
