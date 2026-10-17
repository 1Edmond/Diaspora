using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class HoldFreelanceEscrowCommandHandler(
    IWalletRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> Handle(
        HoldFreelanceEscrowCommand command, CancellationToken cancellationToken)
    {
        var employer = await repository.GetByExternalProfileIdAsync(
            command.EmployerExternalProfileId, cancellationToken);
        if (employer is null)
            return Result.Failure<Guid>("Employer wallet not found.");

        var holdResult = employer.HoldForFreelance(command.Amount, command.JobApplicationId, command.Description);
        if (holdResult.IsFailure)
            return Result.Failure<Guid>(holdResult.Error);

        employer.AddDomainEvent(new FreelanceEscrowHoldCompletedDomainEvent(
            employer.Id, holdResult.Value.Id, command.JobApplicationId, command.Amount, employer.Currency));

        await repository.UpdateAsync(employer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(holdResult.Value.Id);
    }
}