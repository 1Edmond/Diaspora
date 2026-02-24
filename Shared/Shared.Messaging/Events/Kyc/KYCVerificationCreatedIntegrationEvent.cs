using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCVerificationCreatedIntegrationEvent(
    Guid VerificationId,
    Guid UserId
) : IntegrationEvent
{
}
