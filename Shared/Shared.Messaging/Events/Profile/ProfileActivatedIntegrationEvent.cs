using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Profile;

public sealed record ProfileActivatedIntegrationEvent(
    Guid ProfileId,
    Guid ProfileTypeId,
    Guid UserId,
    string CountryCode,
    string Currency,
    DateTime ActivatedAt,
    string? DisplayName = null,
    VerificationLevel Level = VerificationLevel.L0_Registered
) : IntegrationEvent;
