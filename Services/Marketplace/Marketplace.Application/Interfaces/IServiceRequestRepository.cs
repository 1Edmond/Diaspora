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

    /// <summary>
    /// Indique si le demandeur a au moins une demande COMPLÉTÉE sur ce listing.
    /// Sert à déterminer côté serveur si un avis est « vérifié ».
    /// </summary>
    Task<bool> HasCompletedRequestAsync(Guid requesterId, Guid listingId, CancellationToken cancellationToken = default);
}
