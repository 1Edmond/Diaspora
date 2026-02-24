using KYC.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.IntegrationEvents;

public class UserDeletedIntegrationEventHandler(
    IKYCDbContext context,
    ILogger<UserDeletedIntegrationEventHandler> logger)
{
    public async Task Handle(UserDeletedIntegrationEvent integrationEvent)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Handling UserDeleted event - UserId: {UserId}",
                integrationEvent.UserId);

        
        var verifications = await context.KYCVerifications
            .IgnoreQueryFilters()
            .Where(v => v.UserId == Guid.Parse(integrationEvent.UserId))
            .ToListAsync();

        foreach (var verification in verifications)
        {
            verification.Delete(integrationEvent.DeleteBy);
        }

        await context.SaveChangesAsync();
        
        if(logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Deleted {Count} KYC verifications for UserId: {UserId}",
                verifications.Count,
                integrationEvent.UserId);
    }
}
