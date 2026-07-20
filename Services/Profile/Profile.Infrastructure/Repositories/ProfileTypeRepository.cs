using Microsoft.EntityFrameworkCore;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Infrastructure.Repositories;

public class ProfileTypeRepository : IProfileTypeRepository
{

    private readonly ProfileDbContext profileDbContext;

    public ProfileTypeRepository(ProfileDbContext profileDbContext)
    {
        this.profileDbContext = profileDbContext;
    }

    public Task<IEnumerable<ProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ProfileType?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var profile = await profileDbContext.ProfileTypes.FirstOrDefaultAsync(x => x.CountryCode == countryCode);

        return profile;
        
    }
    public async Task<ProfileType?> GetByPhoneIndicatifAsync(string phoneIndicatif, CancellationToken cancellationToken = default)
    {
        var profile = await profileDbContext.ProfileTypes.FirstOrDefaultAsync(x => x.PhoneIndicatif == phoneIndicatif);

        return profile;
        
    }
}
