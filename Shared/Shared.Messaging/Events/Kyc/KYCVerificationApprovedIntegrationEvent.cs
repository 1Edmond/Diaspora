using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCVerificationApprovedIntegrationEvent(
    Guid VerificationId,
    Guid UserId,
    KYCLevel Level
) : IntegrationEvent;
