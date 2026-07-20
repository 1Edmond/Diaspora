using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ProfileKindChangedIntegrationEvent(
    Guid ProfileId,
    Guid UserId,
    string OldKind,
    string NewKind
) : IntegrationEvent;
