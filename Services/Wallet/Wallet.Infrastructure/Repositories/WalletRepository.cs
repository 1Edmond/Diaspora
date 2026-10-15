using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wallet.Application.Interfaces;
using Wallet.Infrastructure.Persistences;

namespace Wallet.Infrastructure.Repositories;

public sealed class WalletRepository(WalletDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Domain.Entities.Wallet>> logger,
    IDistributedCache cache)
    : Repository<Domain.Entities.Wallet>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IWalletRepository
{
    private readonly WalletDbContext _context = context;

    public async Task<Domain.Entities.Wallet?> GetByExternalProfileIdAsync(
        Guid externalProfileId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.ExternalProfileId == externalProfileId, cancellationToken);

    public async Task<Domain.Entities.Wallet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task<bool> ExistsByExternalProfileIdAsync(
        Guid externalProfileId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AnyAsync(w => w.ExternalProfileId == externalProfileId, cancellationToken);

    public async Task<Domain.Entities.Wallet?> GetPlatformRevenueWalletAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        var normalized = currency.ToUpperInvariant();
        return await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w =>
                w.ExternalProfileId == Domain.Entities.Wallet.PlatformProfileId &&
                w.Currency == normalized,
                cancellationToken);
    }

    public async Task<PagedList<Domain.Entities.Wallet>> GetWalletsAdminAsync(
        string? currency = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Wallets
            .Where(w => w.ExternalProfileId != Domain.Entities.Wallet.PlatformProfileId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var normalizedCurrency = currency.ToUpperInvariant();
            query = query.Where(w => w.Currency == normalizedCurrency);
        }

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Domain.Entities.Wallet>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Domain.Entities.Wallet>> GetPlatformWalletsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Where(w => w.ExternalProfileId == Domain.Entities.Wallet.PlatformProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, decimal>> GetTotalHeldBalanceByCurrencyAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Wallets
            .Where(w => w.HeldBalance > 0)
            .GroupBy(w => w.Currency)
            .Select(g => new { Currency = g.Key, TotalHeld = g.Sum(w => w.HeldBalance) })
            .ToDictionaryAsync(x => x.Currency, x => x.TotalHeld, cancellationToken);
    }
}

