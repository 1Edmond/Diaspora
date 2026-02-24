using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCVerificationSubmittedIntegrationEvent(
    Guid VerificationId,
    Guid UserId
) : IntegrationEvent ;