using JasperFx.Core;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using Shared.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Repositories;

public class ServiceRequestRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ServiceRequest>> logger,
    IDistributedCache cache)
    : Repository<ServiceRequest>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IServiceRequestRepository
{
    public async Task<IEnumerable<ServiceRequest>> GetByRequesterAsync(Guid requesterId, CancellationToken cancellationToken = default)
        => await _context.ServiceRequests
            .Include(r => r.Listing)
            .Where(r => r.RequesterId == requesterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ServiceRequest>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await _context.ServiceRequests
            .Include(r => r.Listing)
            .Where(r => r.ProviderId == providerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ServiceRequest>> GetByListingAsync(Guid listingId, CancellationToken cancellationToken = default)
        => await _context.ServiceRequests
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasCompletedRequestAsync(Guid requesterId, Guid listingId, CancellationToken cancellationToken = default)
        => await _context.ServiceRequests
            .AnyAsync(r => r.RequesterId == requesterId
                        && r.ListingId == listingId
                        && (r.Status == ServiceRequestStatus.Completed || r.Status == ServiceRequestStatus.Delivered),
                cancellationToken);
}
