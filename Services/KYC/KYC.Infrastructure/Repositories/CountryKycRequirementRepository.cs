using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence.Repositories;
using Shared.Security.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Infrastructure.Repositories;

public class CountryKycRequirementRepository(
    KYCDbContext context,
    IEncryptionService encryptionService,
    IKeyManagementService keyManagement,
    IEncryptionMetadataRepository metadataRepository,
    ILogger<Repository<CountryKycRequirement>> logger,
    IDistributedCache cache)
    : Repository<CountryKycRequirement>(
        context,
        encryptionService,
        keyManagement,
        metadataRepository,
        logger,
        cache), ICountryKycRequirementRepository
{
    private readonly KYCDbContext _kycContext = context;

    public async Task<IEnumerable<CountryKycRequirement>> GetByProfileTypeIdAsync(Guid profileTypeId, CancellationToken cancellationToken = default)
    {
        return await _kycContext.CountryKycRequirements
            .Where(r => r.ProfileTypeId == profileTypeId)
            .OrderBy(r => r.DocumentTypeCode)
            .ToListAsync(cancellationToken);
    }
}
