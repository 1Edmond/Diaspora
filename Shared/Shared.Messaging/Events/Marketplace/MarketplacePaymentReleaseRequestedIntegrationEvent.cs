using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record MarketplacePaymentReleaseRequestedIntegrationEvent(
    Guid ServiceRequestId,
    Guid? BuyerProfileId = null,
    Guid? SellerProfileId = null,
    decimal CommissionAmount = 0m,
    decimal? Amount = null,
    string? Currency = null
) : IntegrationEvent;
