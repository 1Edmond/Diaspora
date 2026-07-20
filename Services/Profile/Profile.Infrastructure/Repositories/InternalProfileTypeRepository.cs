using Microsoft.EntityFrameworkCore;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Repositories;

public class InternalProfileTypeRepository : IInternalProfileTypeRepository
{

    private readonly ProfileDbContext profileDbContext;

    public InternalProfileTypeRepository(ProfileDbContext profileDbContext)
    {
        this.profileDbContext = profileDbContext;
    }

    public Task<IEnumerable<InternalProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<InternalProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var profile = await profileDbContext.InternalProfileTypes.FirstOrDefaultAsync(x => x.CountryCode == countryCode);

        return profile;
        
    }
    public async Task<InternalProfileType?> GetByPhoneIndicatifAsync(string phoneIndicatif, CancellationToken cancellationToken = default)
    {
        var profile = await profileDbContext.InternalProfileTypes.FirstOrDefaultAsync(x => x.PhoneIndicatif == phoneIndicatif);

        return profile;
        
    }
}
