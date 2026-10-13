using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobMatchFoundIntegrationEvent(
    Guid JobPostingId,
    Guid WorkerId,
    Guid EmployerId,
    Guid CategoryId,
    string Title
) : IntegrationEvent;