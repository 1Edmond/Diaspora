using Shared.Application.Models;
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

    Task<PagedList<Domain.Entities.Wallet>> GetWalletsAdminAsync(
        string? currency = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Domain.Entities.Wallet>> GetPlatformWalletsAsync(
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, decimal>> GetTotalHeldBalanceByCurrencyAsync(
        CancellationToken cancellationToken = default);
}