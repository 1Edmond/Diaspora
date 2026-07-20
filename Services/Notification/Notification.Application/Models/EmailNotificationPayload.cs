using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Models;
public sealed class EmailNotificationPayload
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Cc { get; init; }
}