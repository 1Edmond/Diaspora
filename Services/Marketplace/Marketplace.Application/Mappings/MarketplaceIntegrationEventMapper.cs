using Marketplace.Domain.Events;
using Shared.Domain.Interfaces;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Marketplace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Mappings;

public class MarketplaceIntegrationEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ServiceListingCreatedEvent e => new ServiceListingCreatedIntegrationEvent(
                e.ListingId, e.ProviderId, e.Title),

            ServiceListingApprovedEvent e => new ServiceListingApprovedIntegrationEvent(
                e.ListingId, e.ProviderId),

            ServiceListingRejectedEvent e => new ServiceListingRejectedIntegrationEvent(
                e.ListingId, e.ProviderId, e.Reason),

            ServiceRequestCreatedEvent e => new ServiceRequestCreatedIntegrationEvent(
                e.RequestId, e.ListingId, e.RequesterId, e.ProviderId),

            ServiceRequestAcceptedEvent e => new ServiceRequestAcceptedIntegrationEvent(
                e.RequestId, e.RequesterId, e.ProviderId),

            ServiceRequestCompletedEvent e => new ServiceRequestCompletedIntegrationEvent(
                e.RequestId, e.RequesterId, e.ProviderId, e.AgreedPrice, e.Currency),

            _ => null
        };
    }
}
