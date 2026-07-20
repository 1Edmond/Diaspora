using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Handlers.Commands;

namespace Wallet.Infrastructure.Consumers;

public sealed class ExternalProfileSuspendedConsumer(
    DeactivateWalletCommandHandler handler,
    IIdempotencyService idempotency,
    ILogger<ExternalProfileSuspendedConsumer> logger)
{
    public async Task Handle(ExternalProfileSuspendedIntegrationEvent @event)
    {
        logger.LogInformation(
            "Deactivating wallet for ExternalProfile {ExternalProfileId}", @event.ProfileId);

        await idempotency.ProcessOnceAsync(
            @event.EventId,
            nameof(ExternalProfileSuspendedIntegrationEvent),
            async () =>
            {
                var command = new DeactivateWalletCommand(@event.ProfileId);
                var result = await handler.Handle(command, CancellationToken.None);

                if (result.IsFailure)
                    logger.LogWarning(
                        "Failed to deactivate wallet for ExternalProfile {ExternalProfileId}: {Error}",
                        @event.ProfileId, result.Error);

                return result;
            });
    }
}
