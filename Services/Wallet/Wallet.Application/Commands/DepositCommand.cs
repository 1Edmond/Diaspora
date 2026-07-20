using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Application.Responses;

namespace Wallet.Application.Commands;

public sealed record DepositCommand(
    Guid ExternalProfileId,
    decimal Amount,
    string Description
) : ICommand<Result<DepositResponse>>;