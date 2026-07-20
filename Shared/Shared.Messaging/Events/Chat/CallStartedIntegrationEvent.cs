using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Chat;

public sealed record CallStartedIntegrationEvent(
    Guid CallId,
    Guid ConversationId,
    Guid InitiatorChatProfileId,
    string CallType
) : IntegrationEvent;