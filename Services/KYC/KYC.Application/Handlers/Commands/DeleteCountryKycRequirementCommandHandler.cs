using KYC.Application.Commands;
using KYC.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Handlers.Commands;

public class DeleteCountryKycRequirementCommandHandler(
    ICountryKycRequirementRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeleteCountryKycRequirementCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
            return Result.Failure("Id is required.");

        var requirement = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (requirement is null)
            return Result.Failure("Country KYC requirement not found.");

        await repository.DeleteAsync(requirement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
