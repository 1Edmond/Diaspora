using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record ReviewRepliedIntegrationEvent(
    Guid ReviewId,
    Guid ListingId,
    Guid ReviewerId,
    Guid ProviderId,
    string Response
) : IntegrationEvent;
