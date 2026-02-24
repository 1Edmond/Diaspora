using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Document;

public sealed record DocumentRenewalAlertIntegrationEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    string DocumentTypeName,
    DateTime ExpiresAt,
    int DaysRemaining
) : IntegrationEvent;