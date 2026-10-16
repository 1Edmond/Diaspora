using Microsoft.Extensions.Logging;
using Profile.Application.Commands;
using Profile.Application.Handlers.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Inbox;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Consumers;

public sealed class KycArrivalProofApprovedConsumer
{
    private readonly ActivateProfileCommandHandler? _handler;
    private readonly IProfileRepository? _repository;
    private readonly IUnitOfWork? _unitOfWork;
    private readonly IIdempotencyService _idempotency;
    private readonly ILogger<KycArrivalProofApprovedConsumer> _logger;

    public KycArrivalProofApprovedConsumer(
        IProfileRepository repository,
        IUnitOfWork unitOfWork,
        IIdempotencyService idempotency,
        ILogger<KycArrivalProofApprovedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _idempotency = idempotency;
        _logger = logger;
    }

    public KycArrivalProofApprovedConsumer(
        ActivateProfileCommandHandler handler,
        IIdempotencyService idempotency,
        ILogger<KycArrivalProofApprovedConsumer> logger)
    {
        _handler = handler;
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task Handle(KycArrivalProofApprovedIntegrationEvent message)
    {
        _logger.LogInformation(
            "Received KycArrivalProofApproved for Profile {ProfileId} with level {ApprovedLevel}",
            message.ProfileId,
            message.ApprovedLevel);

        await _idempotency.ProcessOnceAsync(
            message.EventId,
            nameof(KycArrivalProofApprovedIntegrationEvent),
            async () =>
            {
                Result result;
                if (_repository is not null && _unitOfWork is not null)
                {
                    var profile = await _repository.GetByIdAsync(message.ProfileId, CancellationToken.None);
                    if (profile is null)
                    {
                        result = Result.Failure("Profile not found.");
                    }
                    else
                    {
                        result = profile.ApplyKycApproval(message.ApprovedLevel);
                        if (result.IsSuccess)
                        {
                            await _repository.UpdateAsync(profile, CancellationToken.None);
                            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                        }
                    }
                }
                else
                {
                    var command = new ActivateProfileCommand(
                        message.ProfileId,
                        message.ApprovedLevel);

                    result = await _handler!.Handle(command, CancellationToken.None);
                }

                if (result.IsFailure)
                    _logger.LogWarning(
                        "Failed to apply KYC approval to Profile {ProfileId}: {Error}",
                        message.ProfileId,
                        result.Error);

                return result;
            });
    }
}

