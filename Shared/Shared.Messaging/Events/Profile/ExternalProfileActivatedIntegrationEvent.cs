using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ExternalProfileActivatedIntegrationEvent(
    Guid ProfileId,
    Guid UserId,
    string CountryCode,
    DateTime ActivatedAt
) : IntegrationEvent;

