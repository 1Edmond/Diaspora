using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Events.Kyc;

public record KYCDocumentUploadedIntegrationEvent(
    Guid VerificationId,
    Guid UserId,
    Guid DocumentId,
    string DocumentType
) : IntegrationEvent;
