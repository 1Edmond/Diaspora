using Marketplace.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Interfaces;

public interface IMarketplaceParameterRepository
{
    Task<IEnumerable<MarketplaceParameter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MarketplaceParameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task UpdateAsync(MarketplaceParameter parameter, CancellationToken cancellationToken = default);
}
