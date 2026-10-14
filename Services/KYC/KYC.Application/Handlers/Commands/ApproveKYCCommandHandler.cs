using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;

namespace KYC.Application.Handlers.Commands;

public class ApproveKYCCommandHandler(
    IKYCDbContext context,
    IProfileDirectoryRepository? profileDirectoryRepository = null,
    IMessageBus? _messageBus = null)
{
    public async Task<Result> Handle(ApproveKYCCommand command)
    {
        var verification = await context.KYCVerifications
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        if (!Enum.TryParse<KYCLevel>(command.Level, out var level))
            return Result.Failure("Invalid KYC level");

        var result = verification.Approve(command.ReviewerId, level);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        if (_messageBus is not null && profileDirectoryRepository is not null)
        {
            var profile = await profileDirectoryRepository.GetByUserIdAsync(verification.UserId);
            if (profile is not null)
            {
                await _messageBus.PublishAsync(new KycArrivalProofApprovedIntegrationEvent(
                    verification.Id,
                    verification.UserId,
                    profile.Id,
                    profile.CountryCode ?? string.Empty,
                    DateTime.UtcNow));
            }
        }

        return Result.Success();
    }
}