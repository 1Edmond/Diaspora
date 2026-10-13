using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobApplicationCreatedIntegrationEvent(
    Guid JobApplicationId,
    Guid JobPostingId,
    Guid WorkerId,
    Guid EmployerId
) : IntegrationEvent;