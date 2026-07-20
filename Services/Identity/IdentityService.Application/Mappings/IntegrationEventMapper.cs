using IdentityService.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Mappings;

public class IntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            EmailVerifiedEvent e => new EmailVerifiedIntegrationEvent(e.UserId, e.Email),
            PasswordChangedEvent e => new PasswordChangedIntegrationEvent(e.UserId, e.Email), 
            UserCreatedEvent e => new UserCreatedIntegrationEvent(e.UserId, e.Email, e.VerificationCode),
            UserDeletedEvent e => new UserDeletedIntegrationEvent(e.UserId, e.Email, e.DeleteBy),
            UserLockedOutEvent e => new UserLockedOutIntegrationEvent(e.UserId, e.Email, e.LockedUntil),
            UserLoggedInEvent e => new UserLoggedInIntegrationEvent(e.UserId, e.Email, e.LoginAt),
            UserRestoredEvent e => new UserRestoredIntegrationEvent(e.UserId, e.Email),
            UserSuspendedEvent e => new UserSuspendedIntegrationEvent(e.UserId, e.Email, e.Reason),
            UserActivatedEvent e => new UserActivatedIntegrationEvent(e.UserId, e.PhoneNumber, e.FirstName, e.LastName, e.DateOfBirth, e.Email),
            _ => null
        };
    }
}