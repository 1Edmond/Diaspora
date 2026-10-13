using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;

namespace Wallet.Application.Commands;

public sealed record ReleaseFreelanceEscrowCommand(
    Guid EmployerExternalProfileId,
    Guid JobApplicationId,
    decimal Amount,
    string Description
) : ICommand<Result>;