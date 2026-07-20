using Chat.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Consumers;

public sealed class ExternalProfileSuspendedConsumer(
    IChatProfileRepository repository,
    Shared.Domain.Interfaces.IUnitOfWork unitOfWork,
    IIdempotencyService idempotency,
    ILogger<ExternalProfileSuspendedConsumer> logger)
{
    public async Task Handle(ExternalProfileSuspendedIntegrationEvent @event)
    {
        await idempotency.ProcessOnceAsync(
            @event.EventId,
            nameof(ExternalProfileSuspendedIntegrationEvent),
            async () =>
            {
                var profile = await repository
                    .GetByExternalProfileIdAsync(@event.ProfileId, CancellationToken.None);

                if (profile is null)
                {
                    logger.LogWarning(
                        "ChatProfile not found for ExternalProfile {ExternalProfileId}",
                        @event.ProfileId);
                    return Shared.Domain.Commons.Result.Failure("ChatProfile not found.");
                }

                profile.Deactivate();
                await repository.UpdateAsync(profile, CancellationToken.None);
                await unitOfWork.SaveChangesAsync(CancellationToken.None);

                logger.LogInformation(
                    "ChatProfile deactivated for ExternalProfile {ExternalProfileId}",
                    @event.ProfileId);

                return Shared.Domain.Commons.Result.Success();
            });
    }
}
