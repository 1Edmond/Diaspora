using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public sealed record KycArrivalProofApprovedIntegrationEvent(
    Guid KycVerificationId,
    Guid UserId,
    Guid ExternalProfileId,
    string CountryCode,
    DateTime ApprovedAt
) : IntegrationEvent;