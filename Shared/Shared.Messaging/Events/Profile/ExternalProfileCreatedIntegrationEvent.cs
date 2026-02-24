using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ExternalProfileCreatedIntegrationEvent(
    Guid ProfileId,
    Guid InternalProfileId,
    Guid UserId,
    Guid ExternalProfileTypeId,
    string Kind
) : IntegrationEvent;