using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class ActivateExternalProfileCommandHandler(
    IExternalProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        ActivateExternalProfileCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(command.ExternalProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure("External profile not found.");

        var result = profile.Activate(command.KycVerificationId, command.CountryCode);
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
