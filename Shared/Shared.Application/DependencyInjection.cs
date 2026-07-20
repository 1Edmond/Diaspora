using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreApplication(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        var executingAssembly = assembly ?? Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(executingAssembly);


        return services;
    }
}
