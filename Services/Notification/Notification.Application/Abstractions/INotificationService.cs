using Notification.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationService
{
    Task EnqueueNotificationAsync(
        NotificationContext context,
        CancellationToken cancellationToken = default);
}
