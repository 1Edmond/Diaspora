using Notification.Application.Abstractions;
using Notification.Domain.Entities;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Notification.Infrastructure.Mappers;

public class ProfileCreatedPayloadMapper : IInAppNotificationPayloadMapper
{
    public string EventType => nameof(ProfileCreatedIntegrationEvent);

    public InAppNotificationPayload? Map(JsonElement rawPayload)
    {
        var evt = rawPayload.Deserialize<ProfileCreatedIntegrationEvent>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (evt is null)
            return null;

        return new InAppNotificationPayload
        {
            UserId = evt.UserId,
            Title = "Profil créé",
            Body = "Votre profil a été créé avec succès.",
            Data = new Dictionary<string, object>
            {
                ["profileId"] = evt.ProfileId,
                ["phoneNumber"] = evt.PhoneNumber
            }
        };
    }
}