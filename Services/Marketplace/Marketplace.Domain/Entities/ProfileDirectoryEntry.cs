using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;

namespace Marketplace.Domain.Entities;

/// <summary>
/// Projection locale (read model) d'un profil du Profile Service.
///
/// Pourquoi : le jeton d'authentification ne porte que l'identifiant utilisateur (UserId), alors
/// que Wallet, Chat et Documents raisonnent en ProfileId. Le Marketplace garde donc, pour chaque
/// profil, la correspondance UserId ↔ ProfileId ainsi que les informations publiques nécessaires
/// (nom d'affichage, pays, devise, statut de vérification), alimentées par les événements du
/// Profile Service. Aucune donnée n'est écrite ici en dehors des consumers.
///
/// Convention : <c>Id</c> == ProfileId.
/// </summary>
public class ProfileDirectoryEntry : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? CountryCode { get; private set; }   // ISO 3166-1 alpha-2, majuscules
    public string? Currency { get; private set; }      // ISO 4217, majuscules

    /// <summary>Niveau d'unification de vérification (L0 à L3).</summary>
    public VerificationLevel Level { get; private set; } = VerificationLevel.L0_Registered;

    /// <summary>Profil activé (onboarding terminé, preuve KYC approuvée) : seul état où Wallet/Chat existent.</summary>
    public bool IsActivated { get; private set; }

    /// <summary>Identité vérifiée par un administrateur (badge « vérifié »).</summary>
    public bool IsVerified { get; private set; }

    public bool IsSuspended { get; private set; }

    /// <summary>Vérification KYC approuvée.</summary>
    public bool IsKycApproved { get; private set; }

    /// <summary>Un profil peut publier / recevoir des demandes s'il est activé et non suspendu.</summary>
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

    public void ApplyVerification(VerificationLevel level = VerificationLevel.L1_IdentityDeclared)
    {
        IsVerified = true;
        if (level > Level)
            Level = level;
    }

    public void ApplyVerification(DateTime verifiedAt, VerificationLevel level = VerificationLevel.L1_IdentityDeclared)
    {
        IsVerified = true;
        if (level > Level)
            Level = level;
    }

    public void ApplyKycApproval(VerificationLevel approvedLevel = VerificationLevel.L2_IdentityVerified)
    {
        IsKycApproved = true;
        IsVerified = true;
        var targetLevel = approvedLevel >= VerificationLevel.L2_IdentityVerified
            ? approvedLevel
            : VerificationLevel.L2_IdentityVerified;
        if (targetLevel > Level)
            Level = targetLevel;
    }

    public void ApplySuspension() => IsSuspended = true;

    public void SetVerificationLevel(VerificationLevel level)
    {
        Level = level;
    }
}
