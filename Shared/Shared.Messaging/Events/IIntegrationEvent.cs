using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
