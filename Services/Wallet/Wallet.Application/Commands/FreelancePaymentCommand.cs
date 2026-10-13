using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Wallet.Application.Commands;

public sealed record FreelancePaymentCommand(
    Guid EmployerExternalProfileId,
    Guid WorkerExternalProfileId,
    Guid JobApplicationId,
    decimal Amount,
    string Description
) : ICommand<Result>;