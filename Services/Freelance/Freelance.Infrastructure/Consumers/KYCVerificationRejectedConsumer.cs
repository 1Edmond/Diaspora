using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Kyc;
using System;
using System.Threading.Tasks;

namespace Freelance.Infrastructure.Consumers;

public class KYCVerificationRejectedConsumer(
    IWorkerVerificationStatusRepository _verificationRepository,
    ILogger<KYCVerificationRejectedConsumer> logger)
{
    public async Task Handle(KYCVerificationRejectedIntegrationEvent message)
    {
        var status = await _verificationRepository.GetByWorkerIdAsync(message.UserId);
        if (status is null)
        {
            status = WorkerVerificationStatus.Create(message.UserId, kycRequired: true);
            status.Unverify("KYC rejected");
            await _verificationRepository.AddAsync(status);
            logger.LogInformation("Created unverified WorkerVerificationStatus for user {UserId}", message.UserId);
        }
        else
        {
            status.Unverify("KYC rejected");
            await _verificationRepository.UpdateAsync(status);
            logger.LogInformation("Unverified WorkerVerificationStatus for user {UserId}", message.UserId);
        }
    }
}