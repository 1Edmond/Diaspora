using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Responses;

public sealed record DepositResponse
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public decimal Fee { get; init; }
    public decimal BalanceAfter { get; init; }
    public string Currency { get; init; }
}
