using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using Wallet.Application.Commands;
using Wallet.Application.Interfaces;
using Wallet.Domain.Events;

namespace Wallet.Application.Handlers.Commands;

public sealed class RefundFreelanceEscrowCommandHandler(
    IWalletRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        RefundFreelanceEscrowCommand command, CancellationToken cancellationToken)
    {
        var employer = await repository.GetByExternalProfileIdAsync(
            command.EmployerExternalProfileId, cancellationToken);
        if (employer is null)
            return Result.Failure("Employer wallet not found.");

        var refundResult = employer.RefundFreelanceHold(command.Amount, command.JobApplicationId, command.Description);
        if (refundResult.IsFailure)
            return Result.Failure(refundResult.Error);

        employer.AddDomainEvent(new FreelanceEscrowRefundedDomainEvent(
            employer.Id, refundResult.Value.Id, command.JobApplicationId, command.Amount, employer.Currency));

        await repository.UpdateAsync(employer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}