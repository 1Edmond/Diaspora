using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Profile.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProfileApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        RegisterServiceEvent();
        return services;
    }

    public static void RegisterServiceEvent()
    {
        EventTypeRegistry.Register<ProfileCreatedIntegrationEvent>();
    }
    public static void ConfigureProfileDomainQueues(this WolverineOptions options)
    {
        // Exchange sortant — Profile Service publie ses events vers les autres services
        options.PublishMessage<ProfileCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange)
            .UseDurableOutbox();

        options.PublishMessage<ProfileVerifiedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();

        options.PublishMessage<ProfileSuspendedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();

        options.PublishMessage<ExternalProfileCreatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();

        options.PublishMessage<ProfileActivatedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();

        options.PublishMessage<ProfileKindChangedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();

        options.PublishMessage<ExternalProfileSuspendedIntegrationEvent>()
            .ToRabbitExchange(ExchangesConstants.ProfileEventsExchange).UseDurableOutbox();
    }
}
