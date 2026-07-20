using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceRequestAcceptedIntegrationEvent(
    Guid RequestId,
    Guid RequesterId,
    Guid ProviderId
) : IntegrationEvent;