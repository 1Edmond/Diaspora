using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Repositories;

public class MarketplaceParameterRepository : IMarketplaceParameterRepository
{
    private readonly MarketplaceDbContext _context;

    public MarketplaceParameterRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MarketplaceParameter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Parameters
            .OrderBy(p => p.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<MarketplaceParameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Parameters
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
    }

    public async Task UpdateAsync(MarketplaceParameter parameter, CancellationToken cancellationToken = default)
    {
        _context.Parameters.Update(parameter);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
