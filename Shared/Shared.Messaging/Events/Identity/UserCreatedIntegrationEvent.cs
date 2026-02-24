using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Identity;

public record UserCreatedIntegrationEvent( 
    Guid UserId,
    string Email,
    string VerificationCode) : IntegrationEvent
{
   
}