using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Models;

public class PerformanceLogEvent
{
    public string OperationName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public bool IsSlowOperation => DurationMs > 1000;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
}