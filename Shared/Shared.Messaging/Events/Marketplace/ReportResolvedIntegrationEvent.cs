using System;

namespace Shared.Messaging.Events.Marketplace;

public sealed record ReportResolvedIntegrationEvent(
    Guid ReportId,
    Guid ReporterId,
    string TargetType,
    Guid TargetId,
    string Resolution
) : IntegrationEvent;
