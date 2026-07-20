using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Application.Commands;

public sealed record ProcedurePaymentCommand(
    Guid ExternalProfileId,
    Guid ProcedureId,
    decimal Amount,
    string Description
) : ICommand<Result>;
