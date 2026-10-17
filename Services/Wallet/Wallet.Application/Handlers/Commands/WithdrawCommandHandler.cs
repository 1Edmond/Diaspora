using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Responses;

namespace Wallet.Application.Handlers.Commands;

public sealed class WithdrawCommandHandler(
    IWalletRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<WithdrawResponse>> Handle(
        WithdrawCommand command,
        CancellationToken cancellationToken)
    {
        var wallet = await repository.GetByExternalProfileIdAsync(
            command.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<WithdrawResponse>("Wallet not found.");

        var result = wallet.RequestWithdrawal(command.Amount, command.Description);

        if (result.IsFailure)
            return Result.Failure<WithdrawResponse>(result.Error);

        await repository.UpdateAsync(wallet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new WithdrawResponse
        {
            TransactionId = result.Value.Id,
            WalletId = wallet.Id,
            Amount = command.Amount,
            BalanceAfter = wallet.Balance,
            Currency = wallet.Currency,
            Status = result.Value.Status.ToString()
        };

        return Result.Success(response);
    }
}
