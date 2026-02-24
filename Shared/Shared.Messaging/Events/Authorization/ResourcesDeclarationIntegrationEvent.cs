using Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Authorization;

public record ResourcesDeclarationIntegrationEvent : IntegrationEvent
{
    public string ServiceName { get; set; } = string.Empty;
    public List<ResourceDeclaration> Resources { get; set; } = new();
}