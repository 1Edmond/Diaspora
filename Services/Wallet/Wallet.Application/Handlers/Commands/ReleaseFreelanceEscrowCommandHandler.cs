using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class ReleaseFreelanceEscrowCommandHandler(
    IWalletRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        ReleaseFreelanceEscrowCommand command, CancellationToken cancellationToken)
    {
        var employer = await repository.GetByExternalProfileIdAsync(
            command.EmployerExternalProfileId, cancellationToken);
        if (employer is null)
            return Result.Failure("Employer wallet not found.");

        Domain.Entities.Wallet? worker = null;
        if (command.WorkerExternalProfileId.HasValue && command.WorkerExternalProfileId.Value != Guid.Empty)
        {
            worker = await repository.GetByExternalProfileIdAsync(
                command.WorkerExternalProfileId.Value, cancellationToken);
            if (worker is null)
                return Result.Failure("Worker wallet not found.");
        }

        var releaseResult = employer.ReleaseFreelanceHold(
            command.Amount, command.JobApplicationId, command.Description, worker);
        if (releaseResult.IsFailure)
            return Result.Failure(releaseResult.Error);

        employer.AddDomainEvent(new FreelanceEscrowReleasedDomainEvent(
            employer.Id, worker?.Id ?? Guid.Empty, releaseResult.Value.Id, command.JobApplicationId, command.Amount, employer.Currency));

        await repository.UpdateAsync(employer, cancellationToken);
        if (worker is not null)
            await repository.UpdateAsync(worker, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}