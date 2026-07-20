using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceListingApprovedIntegrationEvent(
    Guid ListingId,
    Guid ProviderId
) : IntegrationEvent;