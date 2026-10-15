using KYC.Application.Dtos;
using KYC.Application.Interfaces;
using KYC.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Handlers.Queries;

public class GetCountryKycRequirementsQueryHandler(ICountryKycRequirementRepository repository)
{
    public async Task<Result<List<CountryKycRequirementDto>>> Handle(
        GetCountryKycRequirementsQuery query,
        CancellationToken cancellationToken = default)
    {
        var requirements = query.ProfileTypeId.HasValue && query.ProfileTypeId.Value != Guid.Empty
            ? await repository.GetByProfileTypeIdAsync(query.ProfileTypeId.Value, cancellationToken)
            : await repository.GetAllAsync(cancellationToken);

        var dtos = requirements.Select(r => new CountryKycRequirementDto(
            r.Id,
            r.ProfileTypeId,
            r.DocumentTypeId,
            r.DocumentTypeCode,
            r.Purpose,
            r.IsRequired,
            r.IsActive,
            r.CreatedAt
        )).ToList();

        return Result.Success(dtos);
    }
}
