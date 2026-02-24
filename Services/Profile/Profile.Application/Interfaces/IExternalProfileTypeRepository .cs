using Profile.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Application.Interfaces;


public interface IExternalProfileTypeRepository : IRepository<ExternalProfile>
{
    Task<ExternalProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExternalProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
