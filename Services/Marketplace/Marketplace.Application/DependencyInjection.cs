using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketplaceApplication(this IServiceCollection services)
    {
        // Add any Marketplace-specific services here
        return services;
    }
}
