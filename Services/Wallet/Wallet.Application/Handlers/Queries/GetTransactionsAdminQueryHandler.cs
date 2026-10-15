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

public sealed class GetTransactionsAdminQueryHandler(ITransactionRepository repository)
{
    public async Task<Result<PagedList<TransactionDto>>> Handle(
        GetTransactionsAdminQuery query,
        CancellationToken cancellationToken)
    {
        var pagedTransactions = await repository.GetTransactionsAdminAsync(
            query.WalletId,
            query.Type,
            query.Status,
            query.Currency,
            query.FromDate,
            query.ToDate,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = pagedTransactions.Items.Select(t => t.ToDto()).ToList();

        var result = new PagedList<TransactionDto>(
            dtos,
            pagedTransactions.TotalCount,
            pagedTransactions.PageNumber,
            pagedTransactions.PageSize);

        return Result.Success(result);
    }
}
