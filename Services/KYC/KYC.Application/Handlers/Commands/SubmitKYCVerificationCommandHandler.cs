using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class SubmitKYCVerificationCommandHandler(
    IKYCDbContext context,
    IProfileDirectoryRepository? profileDirectoryRepository = null)
{
    public async Task<Result> Handle(SubmitKYCVerificationCommand command)
    {
        var verification = await context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        var effectiveKycLevel = command.Level ?? (verification.Level != KYCLevel.None ? verification.Level : KYCLevel.Intermediate);
        var targetVerificationLevel = effectiveKycLevel switch
        {
            KYCLevel.Basic => VerificationLevel.L1_IdentityDeclared,
            KYCLevel.Intermediate => VerificationLevel.L2_IdentityVerified,
            KYCLevel.Advanced => VerificationLevel.L3_EnhancedVerified,
            _ => VerificationLevel.L2_IdentityVerified
        };

        List<string>? requiredDocTypes = null;
        if (profileDirectoryRepository is not null)
        {
            var profile = await profileDirectoryRepository.GetByUserIdAsync(verification.UserId);
            if (profile is not null && profile.ProfileTypeId != Guid.Empty)
            {
                var activeRequirements = await context.CountryKycRequirements
                    .Where(r => r.ProfileTypeId == profile.ProfileTypeId && r.IsActive && r.TargetLevel <= targetVerificationLevel)
                    .ToListAsync();

                if (activeRequirements.Count > 0)
                {
                    requiredDocTypes = activeRequirements
                        .Where(r => r.IsRequired)
                        .Select(r => r.DocumentTypeCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var userDocs = verification.Documents;
                    var missingMandatory = requiredDocTypes
                        .Where(code => !userDocs.Any(d => string.Equals(d.DocumentType, code, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (missingMandatory.Count > 0)
                        return Result.Failure($"Missing required documents: {string.Join(", ", missingMandatory)}");

                    var minDistinctDocsRequired = activeRequirements.Max(r => r.RequiredDistinctDocumentsCount);
                    var distinctUserDocsCount = userDocs
                        .Select(d => d.DocumentType.Trim().ToUpperInvariant())
                        .Distinct()
                        .Count();

                    if (distinctUserDocsCount < minDistinctDocsRequired)
                    {
                        return Result.Failure(
                            $"Insufficient distinct documents for level {targetVerificationLevel}: at least {minDistinctDocsRequired} distinct document(s) required, but {distinctUserDocsCount} provided.");
                    }
                }
            }
        }

        var result = verification.Submit(requiredDocTypes);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}