using Marketplace.Domain.Entities;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Interfaces;

public interface IProfileTypePaymentPolicyRepository : IRepository<ProfileTypePaymentPolicy>
{
    Task<ProfileTypePaymentPolicy?> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default);
    Task<ProfileTypePaymentPolicy?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProfileTypePaymentPolicy>> GetAllPoliciesAsync(CancellationToken cancellationToken = default);
    Task<ProfileTypePaymentPolicy> GetOrDefaultAsync(Guid profileTypeId, string? countryCode = null, CancellationToken cancellationToken = default);
}
