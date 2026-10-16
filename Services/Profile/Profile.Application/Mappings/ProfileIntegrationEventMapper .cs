using Profile.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Mappings;

public sealed class ProfileIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        ProfileCreatedDomainEvent e => new ProfileCreatedIntegrationEvent(
            e.ProfileId, e.UserId, e.PhoneNumber, Shared.Domain.Enums.VerificationLevel.L0_Registered, e.ProfileTypeId),

        ProfileVerifiedDomainEvent e => new ProfileVerifiedIntegrationEvent(
            e.ProfileId, e.UserId, e.VerifiedByAdminId, e.VerifiedAt, e.Level),

        ProfileSuspendedDomainEvent e => new ProfileSuspendedIntegrationEvent(
            e.ProfileId, e.UserId),

        ProfileActivatedDomainEvent e => new ProfileActivatedIntegrationEvent(
            e.ProfileId, e.ProfileTypeId, e.UserId, e.CountryCode, e.Currency, e.ActivatedAt, null, e.Level),

        ProfileKindChangedDomainEvent e => new ProfileKindChangedIntegrationEvent(
            e.ProfileId, e.UserId, e.OldKind.ToString(), e.NewKind.ToString()),

        _ => null
    };
}
