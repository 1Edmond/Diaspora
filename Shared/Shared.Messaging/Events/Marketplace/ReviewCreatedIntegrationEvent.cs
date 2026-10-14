using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record ReviewCreatedIntegrationEvent(
    Guid ReviewId,
    Guid ListingId,
    Guid ProviderId,
    Guid ReviewerId,
    int Rating,
    string Comment
) : IntegrationEvent;
