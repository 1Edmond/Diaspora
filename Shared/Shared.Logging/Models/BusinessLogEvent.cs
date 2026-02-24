using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Models;

public class BusinessLogEvent
{
    public string EventType { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime OccurredAt { get; set; }
}