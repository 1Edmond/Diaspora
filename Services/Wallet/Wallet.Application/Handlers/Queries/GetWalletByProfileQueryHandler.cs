using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Dtos;
using Wallet.Application.Extensions;
using Wallet.Application.Interfaces;
using Wallet.Application.Queries;

namespace Wallet.Application.Handlers.Queries;

public sealed class GetWalletByProfileQueryHandler(IWalletRepository repository)
{
    public async Task<Result<WalletDto>> Handle(
        GetWalletByProfileQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await repository.GetByExternalProfileIdAsync(
            query.ExternalProfileId, cancellationToken);

        if (wallet is null)
            return Result.Failure<WalletDto>("Wallet not found.");

        return Result.Success(wallet.ToDto());
    }
}