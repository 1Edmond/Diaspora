using Shared.Domain.Enums;
using System;

namespace KYC.Application.Dtos;

public sealed record CountryKycRequirementDto(
    Guid Id,
    Guid ProfileTypeId,
    Guid DocumentTypeId,
    string DocumentTypeCode,
    string Purpose,
    bool IsRequired,
    bool IsActive,
    DateTime CreatedAt,
    VerificationLevel TargetLevel = VerificationLevel.L2_IdentityVerified,
    int RequiredDistinctDocumentsCount = 1
);
