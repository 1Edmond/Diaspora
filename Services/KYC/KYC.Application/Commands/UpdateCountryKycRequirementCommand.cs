using System;

namespace KYC.Application.Commands;

public sealed record UpdateCountryKycRequirementCommand(
    Guid Id,
    string Purpose,
    bool IsRequired,
    bool IsActive
);
