using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCVerificationRejectedIntegrationEvent(
    Guid VerificationId,
    Guid UserId,
    string Reason
) : IntegrationEvent;
