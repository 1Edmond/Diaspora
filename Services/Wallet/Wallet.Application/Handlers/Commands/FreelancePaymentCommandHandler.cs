using Shared.Domain.Commons;
using System;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class FreelancePaymentCommandHandler(IWalletRepository repository)
{
    public async Task<Result> Handle(
        FreelancePaymentCommand command, CancellationToken cancellationToken)
    {
        var employer = await repository.GetByExternalProfileIdAsync(
            command.EmployerExternalProfileId, cancellationToken);
        if (employer is null)
            return Result.Failure("Employer wallet not found.");

        var worker = await repository.GetByExternalProfileIdAsync(
            command.WorkerExternalProfileId, cancellationToken);
        if (worker is null)
            return Result.Failure("Worker wallet not found.");

        var debitResult = employer.DebitForFreelance(command.Amount, command.JobApplicationId, command.Description);
        if (debitResult.IsFailure)
            return Result.Failure(debitResult.Error);

        var creditResult = worker.CreditFromFreelance(command.Amount, command.JobApplicationId, command.Description);
        if (creditResult.IsFailure)
            return Result.Failure(creditResult.Error);

        employer.AddDomainEvent(new FreelancePaymentCompletedDomainEvent(
            employer.Id, worker.Id, debitResult.Value.Id, command.JobApplicationId, command.Amount, employer.Currency));

        await repository.UpdateAsync(employer, cancellationToken);
        await repository.UpdateAsync(worker, cancellationToken);

        return Result.Success();
    }
}