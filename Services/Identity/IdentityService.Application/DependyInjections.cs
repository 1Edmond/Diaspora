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
        EventTypeRegistry.Register<UserDeletedIntegrationEvent>();
        EventTypeRegistry.Register<UserLockedOutIntegrationEvent>();
        EventTypeRegistry.Register<UserLoggedInIntegrationEvent>();
        EventTypeRegistry.Register<UserRestoredIntegrationEvent>();
        EventTypeRegistry.Register<UserSuspendedIntegrationEvent>();
    }

    public static void ConfigureIdentityDomainQueues(this WolverineOptions options)
    {
        options.PublishMessage<EmailVerifiedIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<PasswordChangedIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserCreatedIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserDeletedIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserLockedOutIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserLoggedInIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserRestoredIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
        options.PublishMessage<UserSuspendedIntegrationEvent>()
                .ToRabbitQueue(QueuesConstants.DefaultRabbitMqQueue)
                .UseDurableOutbox();
    }
}
