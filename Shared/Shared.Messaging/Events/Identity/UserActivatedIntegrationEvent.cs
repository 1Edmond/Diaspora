using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Identity;

public record UserActivatedIntegrationEvent(
    Guid UserId,
    PhoneNumber? PhoneNumber,
    string? FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    string Email) : IntegrationEvent
{

}