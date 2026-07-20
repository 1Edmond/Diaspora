using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;

namespace Wallet.Application.Handlers.Commands;

public sealed class ProcedurePaymentCommandHandler(IWalletRepository repository)
{
    public async Task<Result<Guid>> Handle(
        ProcedurePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var wallet = await repository.GetByExternalProfileIdAsync(
            command.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<Guid>("Wallet not found.");

        var result = wallet.PayProcedure(
            command.Amount, command.ProcedureId, command.Description);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await repository.UpdateAsync(wallet, cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
