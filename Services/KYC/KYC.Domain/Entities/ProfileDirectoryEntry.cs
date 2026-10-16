using Shared.Domain.Primitives;
using System;

namespace KYC.Domain.Entities;

/// <summary>
/// Projection locale des profils pour le service KYC.
/// Permet à KYC de connaître ProfileId et CountryCode d'un UserId pour publier KycArrivalProofApprovedIntegrationEvent.
/// </summary>
public class ProfileDirectoryEntry : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid ProfileTypeId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? CountryCode { get; private set; }
    public string? Currency { get; private set; }
    public bool IsActivated { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsSuspended { get; private set; }

    private ProfileDirectoryEntry() { }

    public static ProfileDirectoryEntry Create(Guid profileId, Guid userId, Guid profileTypeId = default)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("ProfileId is required.", nameof(profileId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        return new ProfileDirectoryEntry
        {
            Id = profileId,
            UserId = userId,
            ProfileTypeId = profileTypeId
        };
    }

    public void SetProfileType(Guid profileTypeId)
    {
        if (profileTypeId != Guid.Empty)
            ProfileTypeId = profileTypeId;
    }

    public void ApplyActivation(string? displayName, string? countryCode, string? currency)
    {
        IsActivated = true;
        IsSuspended = false;

        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName.Trim();
        if (!string.IsNullOrWhiteSpace(countryCode))
            CountryCode = countryCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(currency))
            Currency = currency.Trim().ToUpperInvariant();
    }

    public void ApplyVerification() => IsVerified = true;

    public void ApplySuspension() => IsSuspended = true;
}
