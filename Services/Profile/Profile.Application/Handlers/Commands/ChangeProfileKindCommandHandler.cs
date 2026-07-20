using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Handlers.Commands;

public sealed class ChangeProfileKindCommandHandler(
    IProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        ChangeProfileKindCommand command,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(command.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure("Profile not found.");

        if (profile.UserId != command.UserId)
            return Result.Failure("You are not authorized to modify this profile.");

        var result = profile.ChangeKind(command.NewKind);
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
