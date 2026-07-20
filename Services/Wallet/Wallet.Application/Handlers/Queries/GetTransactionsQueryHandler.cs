using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Dtos;
using Wallet.Application.Extensions;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;

namespace Wallet.Application.Handlers.Queries;

public sealed class GetTransactionsQueryHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<Result<PagedList<TransactionDto>>> Handle(
        GetTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByExternalProfileIdAsync(
            query.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<PagedList<TransactionDto>>("Wallet not found.");

        var transactions = await transactionRepository.GetByWalletIdAsync(
            wallet.Id, query.PageNumber, query.PageSize, cancellationToken);

        var total = await transactionRepository.CountByWalletIdAsync(
            wallet.Id, cancellationToken);

        var dtos = transactions.Select(t => t.ToDto()).ToList();

        var paged = PagedList<TransactionDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}
