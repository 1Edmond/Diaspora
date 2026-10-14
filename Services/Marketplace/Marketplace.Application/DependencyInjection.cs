using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketplaceApplication(this IServiceCollection services)
    {
        // Add any Marketplace-specific services here
        services.AddSingleton<Marketplace.Application.Normalization.IListingDataNormalizer, Marketplace.Application.Normalization.ListingDataNormalizer>();
        return services;
    }
}
