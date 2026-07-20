using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceListingRejectedIntegrationEvent(
    Guid ListingId,
    Guid ProviderId,
    string Reason
) : IntegrationEvent;