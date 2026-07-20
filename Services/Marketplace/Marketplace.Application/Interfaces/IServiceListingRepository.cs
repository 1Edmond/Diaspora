using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Interfaces;

public interface IServiceListingRepository : IRepository<ServiceListing>
{
    Task<IEnumerable<ServiceListing>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceListing>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}