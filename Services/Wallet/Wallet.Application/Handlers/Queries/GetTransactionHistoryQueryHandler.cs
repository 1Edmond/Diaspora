using Shared.Application.Models;
using Shared.Domain.Commons;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Dtos;
using Wallet.Application.Extensions;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;

namespace Wallet.Application.Handlers.Queries;

public sealed class GetTransactionHistoryQueryHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<Result<PagedList<TransactionDto>>> Handle(
        GetTransactionHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByExternalProfileIdAsync(
            query.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<PagedList<TransactionDto>>("Wallet not found.");

        var transactions = await transactionRepository.GetByWalletIdAsync(
            wallet.Id,
            query.PageNumber,
            query.PageSize,
            cancellationToken,
            query.Type,
            query.Status,
            query.Search);

        var total = await transactionRepository.CountByWalletIdAsync(
            wallet.Id,
            cancellationToken,
            query.Type,
            query.Status,
            query.Search);

        var dtos = transactions.Select(t => t.ToDto()).ToList();

        var paged = new PagedList<TransactionDto>(dtos, total, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}
