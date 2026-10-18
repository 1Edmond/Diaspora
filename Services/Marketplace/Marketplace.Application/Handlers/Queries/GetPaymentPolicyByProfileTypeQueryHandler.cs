using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Shared.Domain.Commons;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetPaymentPolicyByProfileTypeQueryHandler(IProfileTypePaymentPolicyRepository _policyRepository)
{
    public async Task<Result<ProfileTypePaymentPolicyDto>> Handle(
        GetPaymentPolicyByProfileTypeQuery query,
        CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetOrDefaultAsync(query.ProfileTypeId, null, cancellationToken);

        var dto = new ProfileTypePaymentPolicyDto(
            policy.Id,
            policy.ProfileTypeId,
            policy.CountryCode,
            policy.AllowedPolicy,
            policy.DefaultCashTiming,
            policy.DirectBookingFeeEnabled,
            policy.DirectBookingFeeFixedAmount,
            policy.DirectBookingFeeRate,
            policy.Currency,
            policy.IsActive
        );

        return Result.Success(dto);
    }
}
