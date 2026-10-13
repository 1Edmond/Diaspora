using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Profile;
using System;
using System.Threading.Tasks;
using Wolverine;

namespace Freelance.Infrastructure.Consumers;

public class ExternalProfileActivatedConsumer(
    IWorkerVerificationStatusRepository _verificationRepository,
    ILogger<ExternalProfileActivatedConsumer> logger,
    IMessageBus messageBus)
{
    public async Task Handle(ProfileActivatedIntegrationEvent message)
    {
        var existing = await _verificationRepository.GetByWorkerIdAsync(message.ProfileId);
        if (existing is null)
        {
            var status = WorkerVerificationStatus.Create(message.ProfileId, kycRequired: false);
            await _verificationRepository.AddAsync(status);
            logger.LogInformation("Created WorkerVerificationStatus for activated profile {ProfileId}", message.ProfileId);
        }
        else if (!existing.KycRequired)
        {
            existing.SetKycRequired(false);
            await _verificationRepository.UpdateAsync(existing);
        }
    }
}