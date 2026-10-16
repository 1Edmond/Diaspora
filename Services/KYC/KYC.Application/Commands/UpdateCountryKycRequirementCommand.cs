using Shared.Domain.Enums;
using System;

namespace KYC.Application.Commands;

public sealed record UpdateCountryKycRequirementCommand(
    Guid Id,
    string Purpose,
    bool IsRequired,
    bool IsActive,
    VerificationLevel TargetLevel = VerificationLevel.L2_IdentityVerified,
    int RequiredDistinctDocumentsCount = 1
);
