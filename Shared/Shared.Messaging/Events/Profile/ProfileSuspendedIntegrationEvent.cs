using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ProfileSuspendedIntegrationEvent(
    Guid ProfileId,
    Guid UserId
) : IntegrationEvent;
