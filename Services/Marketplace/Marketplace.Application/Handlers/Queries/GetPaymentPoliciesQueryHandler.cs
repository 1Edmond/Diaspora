using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Shared.Domain.Commons;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetPaymentPoliciesQueryHandler(IProfileTypePaymentPolicyRepository _policyRepository)
{
    public async Task<Result<List<ProfileTypePaymentPolicyDto>>> Handle(
        GetPaymentPoliciesQuery query,
        CancellationToken cancellationToken)
    {
        var policies = await _policyRepository.GetAllPoliciesAsync(cancellationToken);

        var dtos = policies.Select(p => new ProfileTypePaymentPolicyDto(
            p.Id,
            p.ProfileTypeId,
            p.CountryCode,
            p.AllowedPolicy,
            p.DefaultCashTiming,
            p.DirectBookingFeeEnabled,
            p.DirectBookingFeeFixedAmount,
            p.DirectBookingFeeRate,
            p.Currency,
            p.IsActive
        )).ToList();

        return Result.Success(dtos);
    }
}
