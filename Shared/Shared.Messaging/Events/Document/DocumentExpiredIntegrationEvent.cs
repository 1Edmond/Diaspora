using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Document;

public sealed record DocumentExpiredIntegrationEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    string DocumentTypeName,
    DateTime ExpiredAt
) : IntegrationEvent;