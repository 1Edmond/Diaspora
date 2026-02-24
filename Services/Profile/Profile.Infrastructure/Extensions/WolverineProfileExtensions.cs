using Shared.Messaging.Constants;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Profile.Infrastructure.Extensions;

public static class WolverineProfileExtensions
{
    /// <summary>
    /// Configure les exchanges et queues RabbitMQ spécifiques au Profile Service.
    /// Appelé dans SetCoreInfrastructureServices.
    /// </summary>
    public static void ConfigureProfileDomainQueues(this WolverineOptions options)
    {
        // Exchange sortant — Profile Service publie ses events vers les autres services
        options.PublishMessage<InternalProfileCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<InternalProfileVerifiedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<InternalProfileSuspendedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<ExternalProfileCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<ExternalProfileActivatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<ExternalProfileKindChangedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);

        options.PublishMessage<ExternalProfileSuspendedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange);
    }
}
