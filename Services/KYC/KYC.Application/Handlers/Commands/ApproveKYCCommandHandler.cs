using KYC.Application.Abstractions;
using KYC.Application.Commands;
using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Messaging.Events.Kyc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolverine;

namespace KYC.Application.Handlers.Commands;

public class ApproveKYCCommandHandler(
    IKYCDbContext context,
    IProfileDirectoryRepository? profileDirectoryRepository = null,
    IMessageBus? _messageBus = null)
{
    public async Task<Result> Handle(ApproveKYCCommand command)
    {
        var verification = await context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        if (!Enum.TryParse<KYCLevel>(command.Level, out var level))
            return Result.Failure("Invalid KYC level");

        var approvedVerificationLevel = level switch
        {
            KYCLevel.Basic => VerificationLevel.L1_IdentityDeclared,
            KYCLevel.Intermediate => VerificationLevel.L2_IdentityVerified,
            KYCLevel.Advanced => VerificationLevel.L3_EnhancedVerified,
            _ => VerificationLevel.L2_IdentityVerified
        };

        ProfileDirectoryEntry? profile = null;
        if (profileDirectoryRepository is not null)
        {
            profile = await profileDirectoryRepository.GetByUserIdAsync(verification.UserId);
            if (profile is null)
            {
                return Result.Failure("Le profil citoyen associé à cet utilisateur est introuvable dans l'annuaire KYC. Impossible d'approuver le KYC sans profil synchronisé.");
            }

            if (profile.ProfileTypeId != Guid.Empty)
            {
                var activeRequirements = await context.CountryKycRequirements
                    .Where(r => r.ProfileTypeId == profile.ProfileTypeId && r.IsActive && r.TargetLevel <= approvedVerificationLevel)
                    .ToListAsync();

                if (activeRequirements.Count > 0)
                {
                    var userDocs = verification.Documents;
                    var missingMandatory = activeRequirements
                        .Where(r => r.IsRequired)
                        .Select(r => r.DocumentTypeCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Where(code => !userDocs.Any(d => string.Equals(d.DocumentType, code, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    if (missingMandatory.Count > 0)
                        return Result.Failure($"Cannot approve KYC: missing mandatory document(s) for profile type ({string.Join(", ", missingMandatory)}).");

                    var minDistinctDocsRequired = activeRequirements.Max(r => r.RequiredDistinctDocumentsCount);
                    var distinctUserDocsCount = userDocs
                        .Select(d => d.DocumentType.Trim().ToUpperInvariant())
                        .Distinct()
                        .Count();

                    if (distinctUserDocsCount < minDistinctDocsRequired)
                    {
                        return Result.Failure(
                            $"Cannot approve KYC for level {approvedVerificationLevel}: at least {minDistinctDocsRequired} distinct document(s) required, but {distinctUserDocsCount} provided.");
                    }
                }
            }
        }

        var result = verification.Approve(command.ReviewerId, level);

        if (result.IsFailure)
            return result;

        await context.SaveChangesAsync();

        if (_messageBus is not null && profile is not null)
        {
            await _messageBus.PublishAsync(new KycArrivalProofApprovedIntegrationEvent(
                verification.Id,
                verification.UserId,
                profile.Id,
                profile.CountryCode ?? string.Empty,
                DateTime.UtcNow,
                approvedVerificationLevel));
        }

        return Result.Success();
    }
}