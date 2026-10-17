using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Responses;

namespace Wallet.Application.Handlers.Commands;

public sealed class DepositCommandHandler(
    IWalletRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<DepositResponse>> Handle(
        DepositCommand command,
        CancellationToken cancellationToken)
    {
        var wallet = await repository.GetByExternalProfileIdAsync(
            command.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<DepositResponse>("Wallet not found.");

        var result = wallet.Deposit(command.Amount, command.Description);

        if (result.IsFailure)
            return Result.Failure<DepositResponse>(result.Error);

        await repository.UpdateAsync(wallet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new DepositResponse
        {
            Id = result.Value.Id,
            WalletId = wallet.Id,
            Amount = command.Amount,
            Fee = result.Value.Fee,
            BalanceAfter = wallet.Balance,
            Currency = wallet.Currency
        };
        return Result.Success(response);
    }
}
