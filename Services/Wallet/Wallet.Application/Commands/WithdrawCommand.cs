namespace Wallet.Application.Commands;

public sealed record WithdrawCommand(
    Guid ExternalProfileId,
    decimal Amount,
    string Description);
