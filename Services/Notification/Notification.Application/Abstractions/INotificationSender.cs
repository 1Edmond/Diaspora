using Notification.Application.Models;
using Notification.Domain.Enums;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Abstractions;

public interface INotificationSender
{
    NotificationChannel Channel { get; }
    Task<Result> SendAsync(NotificationContext context);
}
