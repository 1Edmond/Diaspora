using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Document;

public sealed record DocumentRejectedIntegrationEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    string DocumentTypeName,
    string Reason
) : IntegrationEvent;