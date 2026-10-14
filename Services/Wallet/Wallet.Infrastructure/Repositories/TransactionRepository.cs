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
}
