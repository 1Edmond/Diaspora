namespace Wallet.Application.Responses;

public sealed record WithdrawResponse
{
    public Guid TransactionId { get; init; }
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
