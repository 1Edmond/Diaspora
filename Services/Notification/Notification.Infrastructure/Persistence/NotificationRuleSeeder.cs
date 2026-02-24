using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Shared.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Seeders;

public class NotificationRuleSeeder(
    DbContext context,
    ILogger<NotificationRuleSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            if (await context.Set<NotificationRule>().AnyAsync())
            {
                logger.LogInformation("NotificationRules already seeded");
                return;
            }

            logger.LogInformation("Seeding NotificationRules...");

            var rules = GetDefaultNotificationRules();

            await context.Set<NotificationRule>().AddRangeAsync(rules);
            await context.SaveChangesAsync();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("NotificationRules seeded successfully: {Count} rules", rules.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding NotificationRules");
            throw;
        }
    }

    private static List<NotificationRule> GetDefaultNotificationRules()
    {
        return
        [
            new (
                "UserCreatedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Sms
            ),
            new (
                "UserLoggedInIntegrationEvent",
                NotificationChannel.Email
            ),
            new (
                "EmailVerifiedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Sms
            ),
            new (
                "PasswordChangedIntegrationEvent",
                NotificationChannel.Email
            ),
            new (
                "UserLockedOutIntegrationEvent",
                NotificationChannel.Email
            ),
            new (
                "UserSuspendedIntegrationEvent",
                NotificationChannel.Email
            ),


             new (
                "KYCVerificationCreatedIntegrationEvent",
                NotificationChannel.Email
            ),

            new (
                "KYCVerificationSubmittedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Push
            ),

            new (
                "KYCVerificationApprovedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Sms | NotificationChannel.Push
            ),

            new (
                "KYCVerificationRejectedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Sms | NotificationChannel.Push
            ),

            new(
                "KYCMoreInfoRequestedIntegrationEvent",
                NotificationChannel.Email | NotificationChannel.Push
            ),

            new (
                "KYCDocumentUploadedIntegrationEvent",
                NotificationChannel.Email
            ),

            new (
                "KYCDocumentRemovedIntegrationEvent",
                NotificationChannel.Email
            ),

            new (
                "KYCReviewStartedIntegrationEvent",
                NotificationChannel.Email
            ),
        ];
    }
}