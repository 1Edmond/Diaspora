using IdentityService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.Enums;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace IdentityService.Application.Handlers.IntegrationEvents;

public class KYCVerificationRejectedIntegrationEventHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<KYCVerificationRejectedIntegrationEventHandler> _logger;
    private readonly IMessageBus _messageBus;

    public KYCVerificationRejectedIntegrationEventHandler(
        IApplicationDbContext context,
        ILogger<KYCVerificationRejectedIntegrationEventHandler> logger,
        IMessageBus messageBus)
    {
        _context = context;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task Handle(KYCVerificationRejectedIntegrationEvent integrationEvent)
    {
        _logger.LogInformation(
            "Processing KYC Rejected event - UserId: {UserId}, Reason: {Reason}",
            integrationEvent.UserId,
            integrationEvent.Reason);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == integrationEvent.UserId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", integrationEvent.UserId);
            return;
        }

        var result = user.UpdateKYCLevel(KYCLevel.None);

        if (result.IsFailure)
        {
            _logger.LogError(
                "Failed to reset KYC level for user {UserId}: {Error}",
                integrationEvent.UserId,
                result.Error);
            return;
        }

        await _context.SaveChangesAsync();


        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Successfully reset KYC level for user {UserId}",
                integrationEvent.UserId);
        }
    }
}