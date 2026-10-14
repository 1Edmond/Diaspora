using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record ServiceRequestDeclinedIntegrationEvent(
    Guid RequestId,
    Guid ListingId,
    Guid RequesterId,
    Guid ProviderId,
    string Reason
) : IntegrationEvent;
