using Notification.Application.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application;

public static class DependyInjections
{

    public static void AddApplicationDependyInjections(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventMapper, IntegrationEventMapper>();
    }
}
