using KYC.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;

using Shared.Messaging.Events.Kyc;

namespace KYC.Application.Mappers;


public class IntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            KYCDocumentAddedEvent e => new KYCDocumentUploadedIntegrationEvent(e.VerificationId, e.UserId, e.DocumentId, e.DocumentType),
            KYCMoreInfoRequestedEvent e => new KYCMoreInfoRequestedIntegrationEvent(e.VerificationId, e.UserId, e.Message),
            KYCVerificationApprovedEvent e => new KYCVerificationApprovedIntegrationEvent(e.VerificationId, e.UserId, e.Level),
            KYCVerificationCreatedEvent e => new KYCVerificationCreatedIntegrationEvent(e.VerificationId, e.UserId),
            KYCVerificationRejectedEvent e => new KYCVerificationRejectedIntegrationEvent(e.VerificationId, e.UserId, e.Reason),
            KYCVerificationSubmittedEvent e => new KYCVerificationSubmittedIntegrationEvent(e.VerificationId, e.UserId),
            _ => null
        };
    }
}
