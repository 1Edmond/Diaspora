using System;
using System.Collections.Generic;

namespace Wallet.Application.Dtos;

public sealed record PlatformRevenueCurrencySummary(
    string Currency,
    decimal PlatformBalance,
    decimal TotalEscrowHeld
);

public sealed record PlatformRevenueDto(
    List<PlatformRevenueCurrencySummary> Currencies,
    List<WalletDto> PlatformWallets
);
