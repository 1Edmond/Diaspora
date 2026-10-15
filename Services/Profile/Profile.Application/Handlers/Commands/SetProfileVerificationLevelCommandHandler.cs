using Profile.Application.Commands;
using Profile.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.Application.Handlers.Commands;

public sealed class SetProfileVerificationLevelCommandHandler(
    IProfileRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        SetProfileVerificationLevelCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await repository.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure("Profile not found.");

        var result = profile.SetVerificationLevel(request.Level, request.AdminId);
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
