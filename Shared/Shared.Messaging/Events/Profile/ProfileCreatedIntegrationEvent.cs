using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ProfileCreatedIntegrationEvent(
    Guid ProfileId,
    Guid UserId,
    string PhoneNumber
) : IntegrationEvent;
