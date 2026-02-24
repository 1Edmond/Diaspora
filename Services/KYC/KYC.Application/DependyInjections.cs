
using KYC.Application.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;

namespace KYC.Application;

public static class DependyInjections
{

    public static void AddApplicationDependyInjections(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventMapper, IntegrationEventMapper>();
        RegisterServiceEvent();
    }

    public static void RegisterServiceEvent()
    {
        EventTypeRegistry.Register<UserDeletedIntegrationEvent>();
        EventTypeRegistry.Register<UserRestoredIntegrationEvent>();
    }
}
