namespace Marketplace.Application.Abstractions;

public static class RateLimitConstants
{
    // Anti-spam : nombre maximum de listings qu'un provider peut créer sur la fenêtre glissante
    public const int MaxListingsPerWindow = 5;
    public static readonly TimeSpan ListingCreationWindow = TimeSpan.FromHours(24);

    // Anti-spam : nombre maximum d'avis qu'un utilisateur peut poster sur la fenêtre glissante
    public const int MaxReviewsPerWindow = 10;
    public static readonly TimeSpan ReviewCreationWindow = TimeSpan.FromHours(1);
}