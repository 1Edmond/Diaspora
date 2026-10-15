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

    private CountryKycRequirement() { }

    public static CountryKycRequirement Create(
        Guid id,
        Guid profileTypeId,
        Guid documentTypeId,
        string documentTypeCode,
        string purpose,
        bool isRequired = true)
    {
        return new CountryKycRequirement
        {
            Id = id,
            ProfileTypeId = profileTypeId,
            DocumentTypeId = documentTypeId,
            DocumentTypeCode = documentTypeCode.Trim(),
            Purpose = purpose.Trim(),
            IsRequired = isRequired,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string purpose, bool isRequired, bool isActive)
    {
        Purpose = purpose.Trim();
        IsRequired = isRequired;
        IsActive = isActive;
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
