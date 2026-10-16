using KYC.Application.Abstractions;
using KYC.Application.Dtos;
using KYC.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KYC.Application.Handlers.Queries;

public class GetKycComplianceQueryHandler(
    IKYCDbContext context,
    IProfileDirectoryRepository? profileDirectoryRepository = null)
{
    public async Task<Result<KycComplianceDto>> Handle(
        GetKycComplianceQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.UserId == Guid.Empty)
            return Result.Failure<KycComplianceDto>("UserId is required.");

        var effectiveProfileTypeId = query.ProfileTypeId ?? Guid.Empty;
        if (effectiveProfileTypeId == Guid.Empty && profileDirectoryRepository is not null)
        {
            var profile = await profileDirectoryRepository.GetByUserIdAsync(query.UserId, cancellationToken);
            if (profile is not null)
                effectiveProfileTypeId = profile.ProfileTypeId;
        }

        var verification = await context.KYCVerifications
            .Include(v => v.Documents)
            .Where(v => v.UserId == query.UserId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var currentKycStatus = verification?.Status.ToString() ?? "None";
        var approvedLevel = VerificationLevel.L0_Registered;
        if (verification is not null && verification.Status == KYCStatus.Approved)
        {
            approvedLevel = verification.Level switch
            {
                KYCLevel.Basic => VerificationLevel.L1_IdentityDeclared,
                KYCLevel.Intermediate => VerificationLevel.L2_IdentityVerified,
                KYCLevel.Advanced => VerificationLevel.L3_EnhancedVerified,
                _ => VerificationLevel.L2_IdentityVerified
            };
        }

        var userDocs = verification?.Documents ?? Array.Empty<KYC.Domain.Entities.KYCDocument>();
        var submittedDistinctCount = userDocs
            .Select(d => d.DocumentType.Trim().ToUpperInvariant())
            .Distinct()
            .Count();

        var missingMandatory = new List<string>();
        var requiredDistinctCount = 0;

        if (effectiveProfileTypeId != Guid.Empty)
        {
            var requirements = await context.CountryKycRequirements
                .Where(r => r.ProfileTypeId == effectiveProfileTypeId
                    && r.IsActive
                    && r.TargetLevel <= VerificationLevel.L2_IdentityVerified)
                .ToListAsync(cancellationToken);

            if (requirements.Count > 0)
            {
                requiredDistinctCount = requirements.Max(r => r.RequiredDistinctDocumentsCount);
                missingMandatory = requirements
                    .Where(r => r.IsRequired)
                    .Select(r => r.DocumentTypeCode)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(code => !userDocs.Any(d => string.Equals(d.DocumentType, code, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        var isCompliantForActivation = verification is not null
            && verification.Status == KYCStatus.Approved
            && approvedLevel >= VerificationLevel.L2_IdentityVerified
            && missingMandatory.Count == 0
            && submittedDistinctCount >= requiredDistinctCount;

        return Result.Success(new KycComplianceDto(
            query.UserId,
            effectiveProfileTypeId,
            currentKycStatus,
            approvedLevel,
            isCompliantForActivation,
            requiredDistinctCount,
            submittedDistinctCount,
            missingMandatory));
    }
}
