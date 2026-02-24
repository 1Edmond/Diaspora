using IdentityService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace IdentityService.Application.Handlers.IntegrationEvents;

public class KYCVerificationApprovedIntegrationEventHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<KYCVerificationApprovedIntegrationEventHandler> _logger;
    private readonly IMessageBus _messageBus;

    public KYCVerificationApprovedIntegrationEventHandler(
        IApplicationDbContext context,
        ILogger<KYCVerificationApprovedIntegrationEventHandler> logger,
        IMessageBus messageBus)
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task Handle(KYCVerificationApprovedIntegrationEvent integrationEvent)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Processing KYC Approved event - UserId: {UserId}, Level: {Level}",
                integrationEvent.UserId,
                integrationEvent.Level);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == integrationEvent.UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", integrationEvent.UserId);
            return;
        }

        var result = user.UpdateKYCLevel(integrationEvent.Level);

        if (result.IsFailure)
        {
            _logger.LogError(
                "Failed to update KYC level for user {UserId}: {Error}",
                integrationEvent.UserId,
                result.Error);
            return;
        }

        await _context.SaveChangesAsync();

        if(_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Successfully updated KYC level to {Level} for user {UserId}",
                integrationEvent.Level,
                integrationEvent.UserId);
        }
    }
}

