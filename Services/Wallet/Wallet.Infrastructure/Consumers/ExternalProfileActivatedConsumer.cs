using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Handlers.Commands;

namespace Wallet.Infrastructure.Consumers;

public sealed class ExternalProfileActivatedConsumer(
    CreateWalletCommandHandler handler,
    IIdempotencyService idempotency,
    ILogger<ExternalProfileActivatedConsumer> logger)
{
    public async Task Handle(ExternalProfileActivatedIntegrationEvent @event)
    {
        logger.LogInformation(
            "Creating wallet for ExternalProfile {ExternalProfileId}", @event.ProfileId);

        await idempotency.ProcessOnceAsync(
            @event.EventId,
            nameof(ExternalProfileActivatedIntegrationEvent),
            async () =>
            {
                var command = new CreateWalletCommand(
                    @event.ProfileId, @event.UserId, @event.Currency);

                var result = await handler.Handle(command, CancellationToken.None);

                if (result.IsFailure)
                    logger.LogWarning(
                        "Failed to create wallet for ExternalProfile {ExternalProfileId}: {Error}",
                        @event.ProfileId, result.Error);

                return result;
            });
    }
}
