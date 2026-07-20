using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Commands;

public sealed record DeactivateWalletCommand(Guid ExternalProfileId)
    : ICommand<Result>;