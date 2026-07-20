using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Marketplace;

public record ServiceRequestCompletedIntegrationEvent(
    Guid RequestId,
    Guid RequesterId,
    Guid ProviderId,
    decimal? AgreedPrice,
    string? Currency
) : IntegrationEvent;