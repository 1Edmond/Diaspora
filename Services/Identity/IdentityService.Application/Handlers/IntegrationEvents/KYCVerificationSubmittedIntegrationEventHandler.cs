using IdentityService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Handlers.IntegrationEvents;

public class KYCVerificationSubmittedIntegrationEventHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<KYCVerificationSubmittedIntegrationEventHandler> _logger;

    public KYCVerificationSubmittedIntegrationEventHandler(
        IApplicationDbContext context,
        ILogger<KYCVerificationSubmittedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(KYCVerificationSubmittedIntegrationEvent integrationEvent)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Processing KYC Submitted event - UserId: {UserId}, VerificationId: {VerificationId}",
                integrationEvent.UserId,
                integrationEvent.VerificationId);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == integrationEvent.UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", integrationEvent.UserId);
            return;
        }

        
        user.RecordKYCSubmission();

        await _context.SaveChangesAsync();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
            "Recorded KYC submission for user {UserId}",
            integrationEvent.UserId);
        }
    }
}
