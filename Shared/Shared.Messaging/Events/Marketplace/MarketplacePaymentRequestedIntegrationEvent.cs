using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record MarketplacePaymentRequestedIntegrationEvent(
    Guid ServiceRequestId,
    Guid BuyerProfileId,
    Guid SellerProfileId,
    decimal Amount,
    string Currency
) : IntegrationEvent;
