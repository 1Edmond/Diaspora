using Shared.Domain.Interfaces;
using Shared.Messaging.Events;


namespace Notification.Application.Mappings;

public class IntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            _ => null
        };
    }
}