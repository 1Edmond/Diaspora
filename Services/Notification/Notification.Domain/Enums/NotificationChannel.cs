using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Enums;

[Flags]
public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    Whatsapp = 4,
    Telegram = 8,
    Push = 16,
}