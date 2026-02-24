using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredOn { get; init; }
    public string EventType { get; init; }

    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }

    protected IntegrationEvent(Guid eventId, DateTime occurredOn)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
        EventType = GetType().Name;
    }
}