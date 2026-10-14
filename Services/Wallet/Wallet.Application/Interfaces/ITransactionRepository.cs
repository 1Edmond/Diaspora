
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Domain.Entities;
using Wallet.Domain.Enums;

namespace Wallet.Application.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByWalletIdAsync(
        Guid walletId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetByTypeAsync(
        Guid walletId,
        TransactionType type,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetHoldTransactionByReferenceIdAsync(
        Guid serviceRequestId,
        CancellationToken cancellationToken = default);
}
