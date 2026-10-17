using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;

public interface IProfileTypeRepository 
{
    Task<ProfileType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<ProfileType?> GetByPhoneIndicatifAsync(string phoneIndicatif, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfileType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ProfileType profileType, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProfileType profileType, CancellationToken cancellationToken = default);
}
