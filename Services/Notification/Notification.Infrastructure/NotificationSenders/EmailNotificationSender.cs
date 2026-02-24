using Notification.Application.Abstractions;
using Notification.Application.Models;
using Notification.Domain.Enums;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.NotificationSenders;

public class EmailNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<Result> SendAsync(NotificationContext context)
    {
        await Task.Delay(10);

        return Result.Success("Email Send");
    }
}