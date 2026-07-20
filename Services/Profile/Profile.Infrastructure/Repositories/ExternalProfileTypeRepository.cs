using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Repositories;

public class ExternalProfileTypeRepository : IExternalProfileTypeRepository
{
    public Task<IEnumerable<ExternalProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ExternalProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
