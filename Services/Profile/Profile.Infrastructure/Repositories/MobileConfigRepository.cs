using Microsoft.EntityFrameworkCore;
using Profile.Application.Interfaces;
using Profile.Domain.Entities;
using Profile.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories;

public class MobileConfigRepository : IMobileConfigRepository
{
    private readonly ProfileDbContext _context;

    public MobileConfigRepository(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileTypeMobileConfig?> GetGlobalAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MobileConfigs
            .FirstOrDefaultAsync(x => x.ProfileTypeId == null, cancellationToken);
    }

    public async Task<ProfileTypeMobileConfig?> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.MobileConfigs
            .FirstOrDefaultAsync(x => x.ProfileTypeId == profileTypeId, cancellationToken);
    }

    public async Task<ProfileTypeMobileConfig?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        var code = countryCode.ToUpperInvariant();
        return await _context.MobileConfigs
            .FirstOrDefaultAsync(x => x.CountryCode == code, cancellationToken);
    }

    public async Task<IEnumerable<ProfileTypeMobileConfig>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MobileConfigs.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default)
    {
        await _context.MobileConfigs.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default)
    {
        _context.MobileConfigs.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProfileTypeMobileConfig config, CancellationToken cancellationToken = default)
    {
        _context.MobileConfigs.Remove(config);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
