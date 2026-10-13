using Shared.Domain.Commons;
using System;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class ReleaseFreelanceEscrowCommandHandler(IWalletRepository repository)
{
    public async Task<Result> Handle(
        ReleaseFreelanceEscrowCommand command, CancellationToken cancellationToken)
    {
        var employer = await repository.GetByExternalProfileIdAsync(
            command.EmployerExternalProfileId, cancellationToken);
        if (employer is null)
            return Result.Failure("Employer wallet not found.");

        var releaseResult = employer.ReleaseFreelanceHold(command.Amount, command.JobApplicationId, command.Description);
        if (releaseResult.IsFailure)
            return Result.Failure(releaseResult.Error);

        employer.AddDomainEvent(new FreelanceEscrowReleasedDomainEvent(
            employer.Id, Guid.Empty, releaseResult.Value.Id, command.JobApplicationId, command.Amount, employer.Currency));

        await repository.UpdateAsync(employer, cancellationToken);

        return Result.Success();
    }
}