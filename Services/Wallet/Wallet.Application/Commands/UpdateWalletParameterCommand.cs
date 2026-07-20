using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Commands;

public sealed record UpdateWalletParameterCommand(string Key, string Value)
    : ICommand<Result>;
