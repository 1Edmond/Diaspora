using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Kyc;
using System;
using System.Threading.Tasks;

namespace Freelance.Infrastructure.Consumers;

public class KYCVerificationApprovedConsumer(
    IWorkerVerificationStatusRepository _verificationRepository,
    ILogger<KYCVerificationApprovedConsumer> logger)
{
    public async Task Handle(KYCVerificationApprovedIntegrationEvent message)
    {
        var status = await _verificationRepository.GetByWorkerIdAsync(message.UserId);
        if (status is null)
        {
            status = WorkerVerificationStatus.Create(message.UserId, kycRequired: true);
            status.Verify("KYC approved automatically");
            await _verificationRepository.AddAsync(status);
            logger.LogInformation("Created and verified WorkerVerificationStatus for user {UserId}", message.UserId);
        }
        else
        {
            status.Verify("KYC approved");
            await _verificationRepository.UpdateAsync(status);
            logger.LogInformation("Verified WorkerVerificationStatus for user {UserId}", message.UserId);
        }
    }
}