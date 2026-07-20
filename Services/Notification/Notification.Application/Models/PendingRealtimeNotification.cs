using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Models;

public record PendingRealtimeNotification(Guid UserId, long Sequence, Guid NotificationId);