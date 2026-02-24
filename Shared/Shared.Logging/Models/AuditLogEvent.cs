using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Models;

public class AuditLogEvent
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime PerformedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}