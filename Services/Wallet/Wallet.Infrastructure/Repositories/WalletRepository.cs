using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
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
            .FirstOrDefaultAsync(w => w.ExternalProfileId == externalProfileId, cancellationToken);

    public async Task<Domain.Entities.Wallet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public async Task<bool> ExistsByExternalProfileIdAsync(
        Guid externalProfileId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AnyAsync(w => w.ExternalProfileId == externalProfileId, cancellationToken);
}
