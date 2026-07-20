using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Responses;

namespace Wallet.Application.Commands;

public sealed record CreateWalletCommand(
    Guid ExternalProfileId,
    Guid UserId,
    string Currency
) : ICommand<Result<CreateWalletResponse>>;