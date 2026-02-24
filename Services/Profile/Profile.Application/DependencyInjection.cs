using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        //EventTypeRegistry.Register<>();
       
    }
}
