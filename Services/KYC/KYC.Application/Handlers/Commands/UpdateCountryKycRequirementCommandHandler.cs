using KYC.Application.Commands;
using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Handlers.Commands;

public class UpdateCountryKycRequirementCommandHandler(
    ICountryKycRequirementRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CountryKycRequirementDto>> Handle(
        UpdateCountryKycRequirementCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Id == Guid.Empty)
            return Result.Failure<CountryKycRequirementDto>("Id is required.");

        if (string.IsNullOrWhiteSpace(command.Purpose))
            return Result.Failure<CountryKycRequirementDto>("Purpose is required.");

        if (command.RequiredDistinctDocumentsCount < 1)
            return Result.Failure<CountryKycRequirementDto>("RequiredDistinctDocumentsCount must be at least 1.");

        var requirement = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (requirement is null)
            return Result.Failure<CountryKycRequirementDto>("Country KYC requirement not found.");

        requirement.Update(
            command.Purpose,
            command.IsRequired,
            command.IsActive,
            command.TargetLevel,
            command.RequiredDistinctDocumentsCount);

        await repository.UpdateAsync(requirement, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CountryKycRequirementDto(
            requirement.Id,
            requirement.ProfileTypeId,
            requirement.DocumentTypeId,
            requirement.DocumentTypeCode,
            requirement.Purpose,
            requirement.IsRequired,
            requirement.IsActive,
            requirement.CreatedAt,
            requirement.TargetLevel,
            requirement.RequiredDistinctDocumentsCount));
    }
}
