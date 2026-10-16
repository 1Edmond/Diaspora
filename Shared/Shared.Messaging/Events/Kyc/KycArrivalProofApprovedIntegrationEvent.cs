using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public sealed record KycArrivalProofApprovedIntegrationEvent(
    Guid KycVerificationId,
    Guid UserId,
    Guid ProfileId,
    string CountryCode,
    DateTime ApprovedAt,
    VerificationLevel ApprovedLevel = VerificationLevel.L2_IdentityVerified
) : IntegrationEvent;