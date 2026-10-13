using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobApplicationAcceptedIntegrationEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid DecidedBy
) : IntegrationEvent;