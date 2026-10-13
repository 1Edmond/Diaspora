using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobApplicationCompletedIntegrationEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid EmployerId
) : IntegrationEvent;