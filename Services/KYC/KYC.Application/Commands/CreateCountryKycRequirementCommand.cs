using System;

namespace KYC.Application.Commands;

public sealed record CreateCountryKycRequirementCommand(
    Guid ProfileTypeId,
    Guid DocumentTypeId,
    string DocumentTypeCode,
    string Purpose,
    bool IsRequired
);
