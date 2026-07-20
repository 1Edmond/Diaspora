using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;

public interface IProfileTypeRepository 
{
    Task<ProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<ProfileType?> GetByPhoneIndicatifAsync(string phoneIndicatif, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
