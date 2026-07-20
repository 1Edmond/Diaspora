using Chat.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Mappings;

public sealed class ChatIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            MessageSentDomainEvent e => new MessageSentIntegrationEvent(
                e.MessageId, e.ConversationId, e.SenderChatProfileId,
                e.Type.ToString(), e.Content),

            CallStartedDomainEvent e => new CallStartedIntegrationEvent(
                e.CallId, e.ConversationId, e.InitiatorChatProfileId, e.Type.ToString()),

            CallEndedDomainEvent e => new CallEndedIntegrationEvent(
                e.CallId, e.ConversationId, e.FinalStatus.ToString(),
                e.Duration?.TotalSeconds),

            _ => null
        };
}