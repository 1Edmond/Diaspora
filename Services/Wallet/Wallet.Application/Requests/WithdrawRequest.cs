namespace Wallet.Application.Requests;

public sealed record WithdrawRequest(decimal Amount, string Description);
