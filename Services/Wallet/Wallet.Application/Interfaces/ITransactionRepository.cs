
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
        CancellationToken cancellationToken = default,
        TransactionType? type = null,
        TransactionStatus? status = null,
        string? search = null);

    Task<int> CountByWalletIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default,
        TransactionType? type = null,
        TransactionStatus? status = null,
        string? search = null);

    Task<IEnumerable<Transaction>> GetByTypeAsync(
        Guid walletId,
        TransactionType type,
        CancellationToken cancellationToken = default);

    Task<Transaction?> GetHoldTransactionByReferenceIdAsync(
        Guid serviceRequestId,
        CancellationToken cancellationToken = default);

    Task<Shared.Application.Models.PagedList<Transaction>> GetTransactionsAdminAsync(
        Guid? walletId = null,
        TransactionType? type = null,
        TransactionStatus? status = null,
        string? currency = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default,
        string? search = null);
}
