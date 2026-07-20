using Notification.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Notification.Application.Abstractions;

public interface IInAppNotificationPayloadMapper
{
    string EventType { get; }
    InAppNotificationPayload? Map(JsonElement rawPayload);
}