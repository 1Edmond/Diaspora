using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Models;

public class LogContext
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceVersion { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
}
