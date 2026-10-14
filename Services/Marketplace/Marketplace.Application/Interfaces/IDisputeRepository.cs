using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Interfaces;

public interface IDisputeRepository : IRepository<Dispute>
{
    Task<Dispute?> GetByServiceRequestIdAsync(Guid serviceRequestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dispute>> GetActiveDisputesAsync(CancellationToken cancellationToken = default);
}
