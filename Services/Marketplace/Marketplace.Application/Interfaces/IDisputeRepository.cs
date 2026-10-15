using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
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
    Task<PagedList<Dispute>> GetPagedDisputesAsync(DisputeStatus? status = null, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Dispute?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
