using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Application.Responses;

namespace Wallet.Application.Handlers.Commands;

public sealed class CreateWalletCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<CreateWalletResponse>> Handle(
        CreateWalletCommand command,
        CancellationToken cancellationToken)
    {
        var exists = await walletRepository.ExistsByExternalProfileIdAsync(
            command.ExternalProfileId, cancellationToken);

        if (exists)
            return Result.Failure<CreateWalletResponse>(
                "A wallet already exists for this profile.");

        var walletResult = Domain.Entities.Wallet.Create(
            command.ExternalProfileId, command.UserId, command.Currency);

        if (walletResult.IsFailure)
            return Result.Failure<CreateWalletResponse>(walletResult.Error);

        await walletRepository.AddAsync(walletResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateWalletResponse(walletResult.Value.Id));
    }
}
