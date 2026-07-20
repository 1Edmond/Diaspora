using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;

namespace Wallet.Application.Handlers.Commands;

public sealed class UpdateWalletParameterCommandHandler(
    IWalletParameterRepository parameterRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        UpdateWalletParameterCommand command,
        CancellationToken cancellationToken)
    {
        var param = await parameterRepository.GetByKeyAsync(command.Key, cancellationToken);

        if (param is null)
            return Result.Failure($"Parameter '{command.Key}' not found.");

        param.UpdateValue(command.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
