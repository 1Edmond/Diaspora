using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Shared.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Seeders;

public class NotificationRuleSeeder(
    NotificationDbContext context,
    ILogger<NotificationRuleSeeder> logger)
{
    public async Task SeedAsync()
    {
        try
        {
            if (await context.NotificationRules.AnyAsync())
            {
                logger.LogInformation("NotificationRules already seeded");
                return;
            }

            logger.LogInformation("Seeding NotificationRules...");

            var rules = GetDefaultNotificationRules();

            await context.NotificationRules.AddRangeAsync(rules);
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

            // Identity
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


            // Profile
             new (
                "InternalProfileCreatedIntegrationEvent",
                NotificationChannel.Email |  NotificationChannel.InApp
            ),
           


             // KYC
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