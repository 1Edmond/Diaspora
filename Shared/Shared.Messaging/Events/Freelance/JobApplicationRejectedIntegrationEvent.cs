using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobApplicationRejectedIntegrationEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid DecidedBy,
    string? Reason
) : IntegrationEvent;