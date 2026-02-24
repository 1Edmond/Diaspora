using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Identity;

public record EmailVerifiedIntegrationEvent(Guid UserId, string Email) : IntegrationEvent
{

}

