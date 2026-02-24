using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class SuspendExternalProfileCommandHandler(
    IExternalProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        SuspendExternalProfileCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(command.ExternalProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure("External profile not found.");

        var result = profile.Suspend();
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
