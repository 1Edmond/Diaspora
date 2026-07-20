using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceRequestCreatedIntegrationEvent(
    Guid RequestId,
    Guid ListingId,
    Guid RequesterId,
    Guid ProviderId
) : IntegrationEvent;