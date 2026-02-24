using Document.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Document;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Mappings;

public sealed class DocumentIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) => domainEvent switch
    {
        DocumentUploadedDomainEvent e => new DocumentUploadedIntegrationEvent(
            e.DocumentId, e.ExternalProfileId, e.DocumentTypeId,
            e.DocumentTypeName, e.FileName),

        DocumentValidatedDomainEvent e => new DocumentValidatedIntegrationEvent(
            e.DocumentId, e.ExternalProfileId, e.DocumentTypeName, e.ExpiresAt),

        DocumentRejectedDomainEvent e => new DocumentRejectedIntegrationEvent(
            e.DocumentId, e.ExternalProfileId, e.DocumentTypeName, e.Reason),

        DocumentExpiredDomainEvent e => new DocumentExpiredIntegrationEvent(
            e.DocumentId, e.ExternalProfileId, e.DocumentTypeName, e.ExpiredAt),

        DocumentRenewalAlertDomainEvent e => new DocumentRenewalAlertIntegrationEvent(
            e.DocumentId, e.ExternalProfileId, e.DocumentTypeName,
            e.ExpiresAt, e.DaysRemaining),

        _ => null
    };
}