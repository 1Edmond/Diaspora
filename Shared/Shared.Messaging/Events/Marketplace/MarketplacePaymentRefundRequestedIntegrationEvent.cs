using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record MarketplacePaymentRefundRequestedIntegrationEvent(
    Guid ServiceRequestId,
    Guid? BuyerProfileId = null
) : IntegrationEvent;
