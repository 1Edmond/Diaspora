using Marketplace.Application.Commands;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Commands;

public class CreatePaymentPolicyCommandHandler(
    IProfileTypePaymentPolicyRepository _policyRepository,
    IUnitOfWork _unitOfWork)
{
    public async Task<Result<Guid>> Handle(
        CreatePaymentPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var existing = await _policyRepository.GetByProfileTypeIdAsync(command.ProfileTypeId, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>("Une politique de règlement existe déjà pour ce type de profil.");
        }

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
            string.IsNullOrWhiteSpace(command.Currency) ? "EUR" : command.Currency,
            command.IsActive
        );

        await _policyRepository.AddAsync(newPolicy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(newPolicy.Id);
    }
}
