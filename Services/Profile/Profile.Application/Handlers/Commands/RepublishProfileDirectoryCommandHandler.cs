using Microsoft.Extensions.Logging;
using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Profile.Domain.Enums;
using Shared.Domain.Commons;
using Shared.Messaging.Events.Profile;
using Wolverine;

namespace Profile.Application.Handlers.Commands;

public sealed class RepublishProfileDirectoryCommandHandler(
    IProfileRepository repository,
    IMessageBus messageBus,
    ILogger<RepublishProfileDirectoryCommandHandler> logger)
{
    public const int BatchSize = 500;

    public async Task<Result<RepublishProfileDirectoryResponse>> Handle(
        RepublishProfileDirectoryCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting republication of profile directory events in batches of {BatchSize}", BatchSize);

        int totalCount = await repository.GetTotalCountAsync(cancellationToken);
        int totalProfilesProcessed = 0;
        int totalEventsPublished = 0;

        for (int skip = 0; skip < totalCount; skip += BatchSize)
        {
            var batch = await repository.GetBatchWithProfileTypeAsync(skip, BatchSize, cancellationToken);
            if (batch.Count == 0)
                break;

            foreach (var profile in batch)
            {
                try
                {
                    // 1. ProfileCreatedIntegrationEvent (systématique pour tout profil)
                    var phone = profile.PhoneNumber?.GetFullNumber() ?? string.Empty;
                    await messageBus.PublishAsync(new ProfileCreatedIntegrationEvent(
                        profile.Id, profile.UserId, phone));
                    totalEventsPublished++;

                    // 2. ProfileVerifiedIntegrationEvent (si vérifié ou actif ayant une trace de vérification)
                    if (profile.Status == ProfileStatus.Verified || profile.VerifiedAt.HasValue)
                    {
                        await messageBus.PublishAsync(new ProfileVerifiedIntegrationEvent(
                            profile.Id,
                            profile.UserId,
                            profile.VerifiedByAdminId ?? "Admin",
                            profile.VerifiedAt ?? DateTime.UtcNow));
                        totalEventsPublished++;
                    }

                    // 3. ProfileActivatedIntegrationEvent (si profil activé)
                    if (profile.Status == ProfileStatus.Activate)
                    {
                        var displayName = $"{profile.FirstName} {profile.LastName}".Trim();
                        var countryCode = profile.ProfileType?.CountryCode ?? "TG";
                        var currency = profile.ProfileType?.Currency ?? "XOF";
                        var activatedAt = profile.UpdatedAt ?? profile.CreatedAt;

                        await messageBus.PublishAsync(new ProfileActivatedIntegrationEvent(
                            profile.Id,
                            profile.ProfileTypeId,
                            profile.UserId,
                            countryCode,
                            currency,
                            activatedAt,
                            displayName));
                        totalEventsPublished++;
                    }
                    // 4. ProfileSuspendedIntegrationEvent (si suspendu)
                    else if (profile.Status == ProfileStatus.Suspended)
                    {
                        await messageBus.PublishAsync(new ProfileSuspendedIntegrationEvent(
                            profile.Id, profile.UserId));
                        totalEventsPublished++;
                    }

                    totalProfilesProcessed++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error republishing events for profile {ProfileId}", profile.Id);
                }
            }

            logger.LogInformation("Republished batch: processed {Processed}/{Total} profiles ({Events} events so far)",
                totalProfilesProcessed, totalCount, totalEventsPublished);
        }

        logger.LogInformation("Republication finished: {TotalProcessed} profiles processed, {TotalPublished} events published",
            totalProfilesProcessed, totalEventsPublished);

        return Result.Success(new RepublishProfileDirectoryResponse(totalProfilesProcessed, totalEventsPublished));
    }
}
