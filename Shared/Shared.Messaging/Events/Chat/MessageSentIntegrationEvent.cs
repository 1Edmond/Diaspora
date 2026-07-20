using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Chat;

public sealed record MessageSentIntegrationEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderChatProfileId,
    string MessageType,
    string? Content
) : IntegrationEvent;