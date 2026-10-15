using KYC.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Interfaces;

public interface ICountryKycRequirementRepository : IRepository<CountryKycRequirement>
{
    Task<IEnumerable<CountryKycRequirement>> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default);
}
