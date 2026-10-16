using KYC.Application.Commands;
using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Handlers.Commands;

public class CreateCountryKycRequirementCommandHandler(
    ICountryKycRequirementRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CountryKycRequirementDto>> Handle(
        CreateCountryKycRequirementCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProfileTypeId == Guid.Empty)
            return Result.Failure<CountryKycRequirementDto>("ProfileTypeId is required.");

        if (command.DocumentTypeId == Guid.Empty)
            return Result.Failure<CountryKycRequirementDto>("DocumentTypeId is required.");

        if (string.IsNullOrWhiteSpace(command.DocumentTypeCode))
            return Result.Failure<CountryKycRequirementDto>("DocumentTypeCode is required.");

        if (string.IsNullOrWhiteSpace(command.Purpose))
            return Result.Failure<CountryKycRequirementDto>("Purpose is required.");

        if (command.RequiredDistinctDocumentsCount < 1)
            return Result.Failure<CountryKycRequirementDto>("RequiredDistinctDocumentsCount must be at least 1.");

        var existing = await repository.FirstOrDefaultAsync(
            r => r.ProfileTypeId == command.ProfileTypeId && r.DocumentTypeCode == command.DocumentTypeCode.Trim(),
            cancellationToken);

        if (existing is not null)
            return Result.Failure<CountryKycRequirementDto>($"A KYC requirement for document code '{command.DocumentTypeCode}' already exists for this profile type.");

        var requirement = CountryKycRequirement.Create(
            Guid.NewGuid(),
            command.ProfileTypeId,
            command.DocumentTypeId,
            command.DocumentTypeCode,
            command.Purpose,
            command.IsRequired,
            command.TargetLevel,
            command.RequiredDistinctDocumentsCount);

        await repository.AddAsync(requirement, cancellationToken);
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
