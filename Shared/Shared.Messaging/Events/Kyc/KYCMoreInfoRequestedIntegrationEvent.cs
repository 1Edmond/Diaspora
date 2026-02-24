using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCMoreInfoRequestedIntegrationEvent(
    Guid VerificationId,
    Guid UserId,
    string Message
) : IntegrationEvent;
