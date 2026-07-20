using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Entities;

public class InAppNotificationPayload
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
