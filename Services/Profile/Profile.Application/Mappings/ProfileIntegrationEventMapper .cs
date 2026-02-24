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
        InternalProfileCreatedDomainEvent e => new InternalProfileCreatedIntegrationEvent(
            e.ProfileId, e.UserId, e.PhoneNumber),

        InternalProfileVerifiedDomainEvent e => new InternalProfileVerifiedIntegrationEvent(
            e.ProfileId, e.UserId, e.VerifiedByAdminId, e.VerifiedAt),

        InternalProfileSuspendedDomainEvent e => new InternalProfileSuspendedIntegrationEvent(
            e.ProfileId, e.UserId),

        ExternalProfileCreatedDomainEvent e => new ExternalProfileCreatedIntegrationEvent(
            e.ProfileId, e.InternalProfileId, e.UserId, e.ExternalProfileTypeId, e.Kind.ToString()),

        ExternalProfileActivatedDomainEvent e => new ExternalProfileActivatedIntegrationEvent(
            e.ProfileId, e.UserId, e.CountryCode, e.ActivatedAt),

        ExternalProfileKindChangedDomainEvent e => new ExternalProfileKindChangedIntegrationEvent(
            e.ProfileId, e.UserId, e.OldKind.ToString(), e.NewKind.ToString()),

        ExternalProfileSuspendedDomainEvent e => new ExternalProfileSuspendedIntegrationEvent(
            e.ProfileId, e.UserId),

        _ => null
    };
}
