using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Chat;

public sealed record CallEndedIntegrationEvent(
    Guid CallId,
    Guid ConversationId,
    string FinalStatus,
    double? DurationSeconds
) : IntegrationEvent;