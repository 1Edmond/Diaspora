using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;

namespace Wallet.Application.Handlers.Commands;

public sealed class DeactivateWalletCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeactivateWalletCommand command,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByExternalProfileIdAsync(
            command.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure("Wallet not found.");

        wallet.Deactivate();
        await walletRepository.UpdateAsync(wallet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
