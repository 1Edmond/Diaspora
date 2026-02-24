using Microsoft.Extensions.Logging;
using Profile.Application.Commands;
using Profile.Application.Handlers.Commands;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Consumers;

public sealed class KycArrivalProofApprovedConsumer(
    ActivateExternalProfileCommandHandler handler,
    IIdempotencyService idempotency,
    ILogger<KycArrivalProofApprovedConsumer> logger)
{
    public async Task Handle(KycArrivalProofApprovedIntegrationEvent @event)
    {
        logger.LogInformation(
            "Received KycArrivalProofApproved for ExternalProfile {ExternalProfileId}",
            @event.ExternalProfileId);

        await idempotency.ProcessOnceAsync(
            @event.EventId,
            nameof(KycArrivalProofApprovedIntegrationEvent),
            async () =>
            {
                var command = new ActivateExternalProfileCommand(
                    @event.ExternalProfileId,
                    @event.KycVerificationId,
                    @event.CountryCode);

                var result = await handler.Handle(command, CancellationToken.None);

                if (result.IsFailure)
                    logger.LogWarning(
                        "Failed to activate ExternalProfile {ExternalProfileId}: {Error}",
                        @event.ExternalProfileId,
                        result.Error);

                return result;
            });
    }
}

