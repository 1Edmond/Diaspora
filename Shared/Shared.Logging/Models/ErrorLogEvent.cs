using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Models;

public class ErrorLogEvent
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime OccurredAt { get; set; }
}