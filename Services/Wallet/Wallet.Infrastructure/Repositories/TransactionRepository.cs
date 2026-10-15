using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Interfaces;
using Wallet.Domain.Entities;
using Wallet.Domain.Enums;
using Wallet.Infrastructure.Persistences;

namespace Wallet.Infrastructure.Repositories;

public sealed class TransactionRepository(WalletDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<Transaction>> logger,
    IDistributedCache cache)
    : Repository<Transaction>(context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), ITransactionRepository
{
    private readonly WalletDbContext _context = context;

    public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(
        Guid walletId, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .CountAsync(t => t.WalletId == walletId, cancellationToken);

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(
        Guid walletId, TransactionType type,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Where(t => t.WalletId == walletId && t.Type == type)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Transaction?> GetHoldTransactionByReferenceIdAsync(
        Guid serviceRequestId,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.ReferenceId == serviceRequestId &&
                t.Type == TransactionType.MarketplacePaymentHold,
                cancellationToken);

    public async Task<Shared.Application.Models.PagedList<Transaction>> GetTransactionsAdminAsync(
        Guid? walletId = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        string? currency = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.AsNoTracking();

        if (walletId.HasValue)
            query = query.Where(t => t.WalletId == walletId.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var normalizedCurrency = currency.ToUpperInvariant();
            query = query.Where(t => t.Currency == normalizedCurrency);
        }

        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Shared.Application.Models.PagedList<Transaction>(items, totalCount, pageNumber, pageSize);
    }
}
