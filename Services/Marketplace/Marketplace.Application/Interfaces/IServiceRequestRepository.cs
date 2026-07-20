using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Interfaces;

public interface IServiceRequestRepository : IRepository<ServiceRequest>
{
    Task<IEnumerable<ServiceRequest>> GetByRequesterAsync(Guid requesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceRequest>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceRequest>> GetByListingAsync(Guid listingId, CancellationToken cancellationToken = default);
}