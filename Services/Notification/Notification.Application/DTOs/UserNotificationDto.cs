using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.DTOs;

public record UserNotificationDto(
    Guid Id,
    long Sequence,
    string EventType,
    string Payload,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);