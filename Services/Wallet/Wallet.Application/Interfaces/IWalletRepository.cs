using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Interfaces;

public interface IWalletRepository : IRepository<Domain.Entities.Wallet>
{
    Task<Domain.Entities.Wallet?> GetByExternalProfileIdAsync(
        Guid externalProfileId,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.Wallet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByExternalProfileIdAsync(
        Guid externalProfileId,
        CancellationToken cancellationToken = default);

    Task<Domain.Entities.Wallet?> GetPlatformRevenueWalletAsync(
        string currency,
        CancellationToken cancellationToken = default);
}