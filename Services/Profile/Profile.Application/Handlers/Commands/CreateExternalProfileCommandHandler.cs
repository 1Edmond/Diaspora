using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Profile.Application.Responses.Commands;
using Profile.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class CreateExternalProfileCommandHandler(
    IInternalProfileRepository internalRepo,
    IExternalProfileRepository externalRepo,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateExternalProfileResponse>> Handle(
        CreateExternalProfileCommand command,
        CancellationToken cancellationToken)
    {
        var internalProfile = await internalRepo.GetByUserIdAsync(command.UserId, cancellationToken);
        if (internalProfile is null)
            return Result.Failure<CreateExternalProfileResponse>("Internal profile not found.");

        if (!internalProfile.IsVerified)
            return Result.Failure<CreateExternalProfileResponse>(
                "Internal profile must be verified before creating an external profile.");

        var existing = await externalRepo.GetByUserIdAndTypeAsync(
            command.UserId, command.ExternalProfileTypeId, cancellationToken);
        if (existing is not null)
            return Result.Failure<CreateExternalProfileResponse>(
                "An external profile for this country already exists.");

        var profileResult = ExternalProfile.Create(
            internalProfile,
            command.ExternalProfileTypeId,
            command.Kind);

        if (profileResult.IsFailure)
            return Result.Failure<CreateExternalProfileResponse>(profileResult.Error);

        await externalRepo.AddAsync(profileResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateExternalProfileResponse(profileResult.Value.Id));
    }
}

