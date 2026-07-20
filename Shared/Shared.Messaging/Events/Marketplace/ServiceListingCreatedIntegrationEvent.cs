using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceListingCreatedIntegrationEvent(
    Guid ListingId,
    Guid ProviderId,
    string Title
) : IntegrationEvent;
