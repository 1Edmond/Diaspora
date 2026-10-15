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

public sealed class GetWalletsAdminQueryHandler(IWalletRepository repository)
{
    public async Task<Result<PagedList<WalletDto>>> Handle(
        GetWalletsAdminQuery query,
        CancellationToken cancellationToken)
    {
        var pagedWallets = await repository.GetWalletsAdminAsync(
            query.Currency,
            query.IsActive,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = pagedWallets.Items.Select(w => w.ToDto()).ToList();

        var result = new PagedList<WalletDto>(
            dtos,
            pagedWallets.TotalCount,
            pagedWallets.PageNumber,
            pagedWallets.PageSize);

        return Result.Success(result);
    }
}
