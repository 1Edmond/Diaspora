using Shared.Domain.Enums;
using System;
using System.Collections.Generic;

namespace KYC.Application.Dtos;

public sealed record KycComplianceDto(
    Guid UserId,
    Guid ProfileTypeId,
    string CurrentKycStatus,
    VerificationLevel ApprovedVerificationLevel,
    bool IsCompliantForActivation,
    int RequiredDistinctDocumentsCount,
    int SubmittedDistinctDocumentsCount,
    IReadOnlyList<string> MissingMandatoryDocuments
);
