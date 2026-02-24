using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Models;

public class NotificationContext
{
    public Guid MessageId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public object Payload { get; init; }
}
