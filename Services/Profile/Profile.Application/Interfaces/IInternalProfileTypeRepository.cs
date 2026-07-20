using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;

public interface IInternalProfileTypeRepository 
{
    Task<InternalProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<InternalProfileType?> GetByPhoneIndicatifAsync(string phoneIndicatif, CancellationToken cancellationToken = default);
    Task<IEnumerable<InternalProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
