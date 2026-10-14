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

public class CategoryRepository(MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Category>> logger,
    IDistributedCache cache)
    : Repository<Category>(_context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache
        ), ICategoryRepository
{
    public override async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Categories
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
}
