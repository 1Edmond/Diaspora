using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Application.Dtos;
using Wallet.Application.Extensions;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;

namespace Wallet.Application.Handlers.Queries;

public sealed class GetPlatformRevenueQueryHandler(IWalletRepository repository)
{
    public async Task<Result<PlatformRevenueDto>> Handle(
        GetPlatformRevenueQuery query,
        CancellationToken cancellationToken)
    {
        var platformWallets = (await repository.GetPlatformWalletsAsync(cancellationToken)).ToList();
        var heldBalances = await repository.GetTotalHeldBalanceByCurrencyAsync(cancellationToken);

        var currencies = platformWallets.Select(w => w.Currency)
            .Union(heldBalances.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        var summaries = currencies.Select(currency =>
        {
            var platformBalance = platformWallets
                .Where(w => string.Equals(w.Currency, currency, StringComparison.OrdinalIgnoreCase))
                .Sum(w => w.Balance);

            heldBalances.TryGetValue(currency, out var totalHeld);

            return new PlatformRevenueCurrencySummary(currency, platformBalance, totalHeld);
        }).ToList();

        var platformDtos = platformWallets.Select(w => w.ToDto()).ToList();

        return Result.Success(new PlatformRevenueDto(summaries, platformDtos));
    }
}
