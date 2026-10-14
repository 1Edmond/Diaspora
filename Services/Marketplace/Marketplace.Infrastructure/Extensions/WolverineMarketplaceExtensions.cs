using Shared.Messaging.Constants;
using Shared.Messaging.Events.Marketplace;
using Shared.Messaging.Events.Wallet;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Marketplace.Infrastructure.Extensions;

public static class WolverineMarketplaceExtensions
{
    public static void ConfigureMarketplaceDomainQueues(this WolverineOptions options)
    {
        // ── Publish — événements publiés vers RabbitMQ ─────────────────────────
        options.PublishMessage<ServiceListingCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<ServiceListingApprovedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<ServiceListingRejectedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<ServiceRequestCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<ServiceRequestAcceptedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<ServiceRequestCompletedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<MarketplacePaymentRequestedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<MarketplacePaymentReleaseRequestedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);

        options.PublishMessage<MarketplacePaymentRefundRequestedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.MarketplaceEventsExchange);
    }
}
