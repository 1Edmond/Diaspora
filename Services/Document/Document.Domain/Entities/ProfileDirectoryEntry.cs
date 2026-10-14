using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace Document.Domain.Entities;

/// <summary>
/// Projection locale (read model) d'un profil du Profile Service pour le service Document.
/// Permet de résoudre la correspondance UserId (du jeton JWT) ↔ ProfileId (des documents),
/// sans appel HTTP synchrone.
/// Convention : Id == ProfileId.
/// </summary>
public class ProfileDirectoryEntry : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? CountryCode { get; private set; }
    public string? Currency { get; private set; }

    /// <summary>Niveau d'unification de vérification (L0 à L3).</summary>
    public VerificationLevel Level { get; private set; } = VerificationLevel.L0_Registered;

    public bool IsActivated { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsSuspended { get; private set; }
    public bool IsKycApproved { get; private set; }

    public bool IsEligibleProvider => IsActivated && !IsSuspended;

    private ProfileDirectoryEntry() { }

    public static ProfileDirectoryEntry Create(Guid profileId, Guid userId)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("ProfileId is required.", nameof(profileId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        return new ProfileDirectoryEntry
        {
            Id = profileId,
            UserId = userId,
            Level = VerificationLevel.L0_Registered
        };
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

    public void ApplyVerification()
    {
        IsVerified = true;
        Level = VerificationLevel.L2_IdentityVerified;
    }

    public void ApplyKycApproval()
    {
        IsKycApproved = true;
        IsVerified = true;
        Level = VerificationLevel.L2_IdentityVerified;
    }

    public void ApplySuspension() => IsSuspended = true;

    public void SetVerificationLevel(VerificationLevel level)
    {
        Level = level;
    }
}
