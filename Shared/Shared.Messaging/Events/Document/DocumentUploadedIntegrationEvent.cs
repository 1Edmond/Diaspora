using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Document;

public sealed record DocumentUploadedIntegrationEvent(
    Guid DocumentId,
    Guid ExternalProfileId,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string FileName
) : IntegrationEvent;