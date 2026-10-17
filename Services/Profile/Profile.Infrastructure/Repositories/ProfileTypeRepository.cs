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

    public async Task<ProfileType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await profileDbContext.ProfileTypes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProfileType>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await profileDbContext.ProfileTypes
            .Where(x => x.IsActive)
            .OrderBy(x => x.CountryName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProfileType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await profileDbContext.ProfileTypes
            .OrderBy(x => x.CountryName)
            .ToListAsync(cancellationToken);
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

    public async Task AddAsync(ProfileType profileType, CancellationToken cancellationToken = default)
    {
        await profileDbContext.ProfileTypes.AddAsync(profileType, cancellationToken);
        await profileDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProfileType profileType, CancellationToken cancellationToken = default)
    {
        profileDbContext.ProfileTypes.Update(profileType);
        await profileDbContext.SaveChangesAsync(cancellationToken);
    }
}
