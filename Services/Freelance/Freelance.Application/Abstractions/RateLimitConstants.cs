namespace Freelance.Application.Abstractions;

public static class RateLimitConstants
{
    // Anti-spam : nombre maximum de job postings qu'un employeur peut créer sur la fenêtre glissante
    public const int MaxJobPostingsPerWindow = 10;
    public static readonly TimeSpan JobPostingCreationWindow = TimeSpan.FromHours(24);

    // Anti-spam : nombre maximum de candidatures qu'un worker peut envoyer sur la fenêtre glissante
    public const int MaxApplicationsPerWindow = 20;
    public static readonly TimeSpan ApplicationCreationWindow = TimeSpan.FromHours(1);
}