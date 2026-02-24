using KYC.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Identity;

namespace KYC.Application.Handlers.IntegrationEvents;

public class UserRestoredIntegrationEventHandler(
    IKYCDbContext context,
    ILogger<UserRestoredIntegrationEventHandler> logger)
{
    public async Task Handle(UserRestoredIntegrationEvent integrationEvent)
    {
        if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                "Handling UserRestored event - UserId: {UserId}",
                integrationEvent.UserId);

        var verifications = await context.KYCVerifications
            .IgnoreQueryFilters()
            .Where(v => v.UserId == Guid.Parse(integrationEvent.UserId) && v.IsDeleted)
            .ToListAsync();

        foreach (var verification in verifications)
        {
            verification.Restore();
        }

        await context.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Restored {Count} KYC verifications for UserId: {UserId}",
                verifications.Count,
                integrationEvent.UserId);
    }
}
