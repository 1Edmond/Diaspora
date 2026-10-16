using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ProfileVerifiedIntegrationEvent(
    Guid ProfileId,
    Guid UserId,
    string VerifiedByAdminId,
    DateTime VerifiedAt,
    VerificationLevel Level = VerificationLevel.L1_IdentityDeclared
) : IntegrationEvent;
