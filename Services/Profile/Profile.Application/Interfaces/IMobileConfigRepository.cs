using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.Application.Interfaces;

public interface IMobileConfigRepository
{
    Task<ProfileTypeMobileConfig?> GetGlobalAsync(CancellationToken cancellationToken = default);
    Task<ProfileTypeMobileConfig?> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default);
    Task<ProfileTypeMobileConfig?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfileTypeMobileConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default);
}
