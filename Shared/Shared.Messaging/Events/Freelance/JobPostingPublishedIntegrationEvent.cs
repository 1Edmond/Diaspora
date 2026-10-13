using Shared.Messaging.Events;
using System;

namespace Shared.Messaging.Events.Freelance;

public record JobPostingPublishedIntegrationEvent(
    Guid JobPostingId,
    Guid EmployerId,
    Guid CategoryId,
    string Title,
    string? City,
    string? Country,
    string[] RequiredSkills
) : IntegrationEvent;