using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace KYC.Domain.Entities;

public class CountryKycRequirement : AggregateRoot
{
    public Guid ProfileTypeId { get; private set; }
    public Guid DocumentTypeId { get; private set; }
    public string DocumentTypeCode { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public bool IsActive { get; private set; }
    public VerificationLevel TargetLevel { get; private set; } = VerificationLevel.L2_IdentityVerified;
    public int RequiredDistinctDocumentsCount { get; private set; } = 1;

    private CountryKycRequirement() { }

    public static CountryKycRequirement Create(
        Guid id,
        Guid profileTypeId,
        Guid documentTypeId,
        string documentTypeCode,
        string purpose,
        bool isRequired = true,
        VerificationLevel targetLevel = VerificationLevel.L2_IdentityVerified,
        int requiredDistinctDocumentsCount = 1)
    {
        if (requiredDistinctDocumentsCount < 1)
            throw new ArgumentException("RequiredDistinctDocumentsCount must be at least 1.", nameof(requiredDistinctDocumentsCount));

        return new CountryKycRequirement
        {
            Id = id,
            ProfileTypeId = profileTypeId,
            DocumentTypeId = documentTypeId,
            DocumentTypeCode = documentTypeCode.Trim(),
            Purpose = purpose.Trim(),
            IsRequired = isRequired,
            IsActive = true,
            TargetLevel = targetLevel,
            RequiredDistinctDocumentsCount = requiredDistinctDocumentsCount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string purpose,
        bool isRequired,
        bool isActive,
        VerificationLevel targetLevel = VerificationLevel.L2_IdentityVerified,
        int requiredDistinctDocumentsCount = 1)
    {
        if (requiredDistinctDocumentsCount < 1)
            throw new ArgumentException("RequiredDistinctDocumentsCount must be at least 1.", nameof(requiredDistinctDocumentsCount));

        Purpose = purpose.Trim();
        IsRequired = isRequired;
        IsActive = isActive;
        TargetLevel = targetLevel;
        RequiredDistinctDocumentsCount = requiredDistinctDocumentsCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
