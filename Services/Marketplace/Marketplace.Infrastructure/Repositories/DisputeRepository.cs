using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Repositories;

public class DisputeRepository(
    MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Dispute>> logger,
    IDistributedCache cache)
    : Repository<Dispute>(
        _context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IDisputeRepository
{
    public async Task<Dispute?> GetByServiceRequestIdAsync(Guid serviceRequestId, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .FirstOrDefaultAsync(d => d.ServiceRequestId == serviceRequestId, cancellationToken);
    }

    public async Task<IEnumerable<Dispute>> GetActiveDisputesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .Where(d => d.Status == DisputeStatus.Opened || d.Status == DisputeStatus.UnderReview)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<Dispute>> GetPagedDisputesAsync(
        DisputeStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Disputes
            .Include(d => d.ServiceRequest)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Dispute>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Dispute?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Disputes
            .Include(d => d.ServiceRequest)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
