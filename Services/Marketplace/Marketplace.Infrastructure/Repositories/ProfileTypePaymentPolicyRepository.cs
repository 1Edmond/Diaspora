using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Infrastructure.Repositories;

public class ProfileTypePaymentPolicyRepository(
    MarketplaceDbContext _context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<ProfileTypePaymentPolicy>> logger,
    IDistributedCache cache)
    : Repository<ProfileTypePaymentPolicy>(
        _context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), IProfileTypePaymentPolicyRepository
{
    public async Task<ProfileTypePaymentPolicy?> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentPolicies
            .FirstOrDefaultAsync(p => p.ProfileTypeId == profileTypeId, cancellationToken);
    }

    public async Task<ProfileTypePaymentPolicy?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        var normalized = countryCode.Trim().ToUpperInvariant();
        return await _context.PaymentPolicies
            .FirstOrDefaultAsync(p => p.CountryCode == normalized, cancellationToken);
    }

    public async Task<IEnumerable<ProfileTypePaymentPolicy>> GetAllPoliciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentPolicies
            .OrderBy(p => p.CountryCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfileTypePaymentPolicy> GetOrDefaultAsync(Guid profileTypeId, string? countryCode = null, CancellationToken cancellationToken = default)
    {
        ProfileTypePaymentPolicy? policy = null;

        if (profileTypeId != Guid.Empty)
        {
            policy = await GetByProfileTypeIdAsync(profileTypeId, cancellationToken);
        }

        if (policy is null && !string.IsNullOrWhiteSpace(countryCode))
        {
            policy = await GetByCountryCodeAsync(countryCode, cancellationToken);
        }

        if (policy is not null)
            return policy;

        // Default fallback policy (Both allowed, AtPickup default, direct booking fees disabled)
        var code = !string.IsNullOrWhiteSpace(countryCode) ? countryCode.Trim().ToUpperInvariant() : "GLOBAL";
        var id = profileTypeId != Guid.Empty ? profileTypeId : Guid.NewGuid();

        return ProfileTypePaymentPolicy.Create(
            profileTypeId: id,
            countryCode: code,
            allowedPolicy: AllowedSettlementPolicy.Both,
            defaultCashTiming: DirectCashTiming.AtPickup,
            directBookingFeeEnabled: false,
            directBookingFeeFixedAmount: 0m,
            directBookingFeeRate: 0m,
            currency: "EUR",
            isActive: true
        );
    }
}
