using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class UpdatePaymentPolicyCommandHandler(
    IProfileTypePaymentPolicyRepository _policyRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result> Handle(
        UpdatePaymentPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await _policyRepository.GetByProfileTypeIdAsync(command.ProfileTypeId, cancellationToken);

        if (existing is null)
        {
            var countryCode = !string.IsNullOrWhiteSpace(command.CountryCode)
                ? command.CountryCode
                : "GLOBAL";

            var newPolicy = ProfileTypePaymentPolicy.Create(
                command.ProfileTypeId,
                countryCode,
                command.AllowedPolicy,
                command.DefaultCashTiming,
                command.DirectBookingFeeEnabled,
                command.DirectBookingFeeFixedAmount,
                command.DirectBookingFeeRate,
                command.Currency,
                command.IsActive
            );

            await _policyRepository.AddAsync(newPolicy, cancellationToken);
        }
        else
        {
            existing.Update(
                command.AllowedPolicy,
                command.DefaultCashTiming,
                command.DirectBookingFeeEnabled,
                command.DirectBookingFeeFixedAmount,
                command.DirectBookingFeeRate,
                command.Currency,
                command.IsActive
            );

            await _policyRepository.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
