using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Infrastructure.Repositories;

public class ServiceListingRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ServiceListing>> logger,
    IDistributedCache cache)
    : Repository<ServiceListing>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IServiceListingRepository
{
    public async Task<IEnumerable<ServiceListing>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await _context.ServiceListings
            .Where(l => l.ProviderId == providerId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ServiceListing>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await _context.ServiceListings
            .Where(l => l.CategoryId == categoryId && l.IsActive)
            .ToListAsync(cancellationToken);
}