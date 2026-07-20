using IdentityService.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Constants;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;
using Wolverine;
using Wolverine.RabbitMQ;

namespace IdentityService.Application;

public static class DependyInjections
{

    public static void AddApplicationDependyInjections(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventMapper, IntegrationEventMapper>();
        RegisterServiceEvent();
    }

    public static void RegisterServiceEvent()
    {
        EventTypeRegistry.Register<EmailVerifiedIntegrationEvent>();
        EventTypeRegistry.Register<PasswordChangedIntegrationEvent>();
        EventTypeRegistry.Register<UserCreatedIntegrationEvent>();
        EventTypeRegistry.Register<UserActivatedIntegrationEvent>();
        EventTypeRegistry.Register<UserDeletedIntegrationEvent>();
        EventTypeRegistry.Register<UserLockedOutIntegrationEvent>();
        EventTypeRegistry.Register<UserLoggedInIntegrationEvent>();
        EventTypeRegistry.Register<UserRestoredIntegrationEvent>();
        EventTypeRegistry.Register<UserSuspendedIntegrationEvent>();
    }

    public static void ConfigureIdentityDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<EmailVerifiedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<PasswordChangedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserCreatedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserDeletedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserLockedOutIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserLoggedInIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserRestoredIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserSuspendedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
        options.PublishMessage<UserActivatedIntegrationEvent>()
                .ToRabbitExchange(ExchangesConstants.IdentityEventsExchange, (s) => s.ExchangeType = ExchangeType.Fanout)
                .UseDurableOutbox();
    }
}
