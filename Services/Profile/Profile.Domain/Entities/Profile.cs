using Profile.Domain.Enums;
using Profile.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class Profile : AggregateRoot
{
    public Guid UserId { get; private set; }

    public Guid ProfileTypeId { get; private set; }
    public ProfileType ProfileType { get; private set; } = null!;

    // Phone number with Togo indicatif — stored as EncryptedString in infra
    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }

    public ProfileStatus Status { get; private set; }

    public Shared.Domain.Enums.VerificationLevel Level { get; private set; } = Shared.Domain.Enums.VerificationLevel.L0_Registered;

    public ProfileKind Kind { get; private set; }

    public DateTime? VerifiedAt { get; private set; }
    public string? VerifiedByAdminId { get; private set; }

    private Profile() { }

    private Profile(
        Guid id,
        Guid userId,
        Guid profileTypeId,
        PhoneNumber phoneNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth) : base(id)
    {
        UserId = userId;
        PhoneNumber = phoneNumber;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        ProfileTypeId = profileTypeId;
        Status = ProfileStatus.PendingVerification;
    }

    public static Result<Profile> Create(
        Guid userId,
        Guid ProfileTypeId,
        PhoneNumber phoneNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        if (userId == Guid.Empty)
            return Result.Failure<Profile>("UserId is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Profile>("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Profile>("Last name is required.");

        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15)))
            return Result.Failure<Profile>("User must be at least 15 years old.");



        var profile = new Profile(
            Guid.NewGuid(),
            userId,
            ProfileTypeId,
            phoneNumber,
            firstName.Trim(),
            lastName.Trim(),
            dateOfBirth);

        profile.AddDomainEvent(new ProfileCreatedDomainEvent(
            profile.Id,
            userId,
            phoneNumber.GetFullNumber(),
            ProfileTypeId));

        return Result.Success(profile);
    }

    /// <summary>
    /// Called by an admin to validate the user's declared identity (L1).
    /// Transitions status to Verified with VerificationLevel L1_IdentityDeclared.
    /// </summary>
    public Result Verify(string adminId)
    {
        if (Status == ProfileStatus.Suspended)
            return Result.Failure("Cannot verify a suspended profile.");

        if (string.IsNullOrWhiteSpace(adminId))
            return Result.Failure("Admin ID is required.");

        Status = ProfileStatus.Verified;
        if (Level < Shared.Domain.Enums.VerificationLevel.L1_IdentityDeclared)
            Level = Shared.Domain.Enums.VerificationLevel.L1_IdentityDeclared;

        VerifiedAt = DateTime.UtcNow;
        VerifiedByAdminId = adminId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProfileVerifiedDomainEvent(
            Id, UserId, adminId, VerifiedAt.Value, Shared.Domain.Enums.VerificationLevel.L1_IdentityDeclared));

        return Result.Success();
    }

    public Result SetVerificationLevel(Shared.Domain.Enums.VerificationLevel level, string? adminId = null)
    {
        if (Status == ProfileStatus.Suspended)
            return Result.Failure("Cannot change verification level of a suspended profile.");

        if (level >= Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified && Status == ProfileStatus.PendingVerification)
            return Result.Failure("Le profil doit d'abord être vérifié (L1 - Identité déclarée) avant de pouvoir atteindre le niveau L2 ou L3.");

        Level = level;
        if (level >= Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified)
        {
            VerifiedAt ??= DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(adminId))
                VerifiedByAdminId = adminId;

            AddDomainEvent(new ProfileVerifiedDomainEvent(
                Id, UserId, VerifiedByAdminId ?? adminId ?? "Admin", VerifiedAt.Value, level));
        }
        else if (level == Shared.Domain.Enums.VerificationLevel.L1_IdentityDeclared && Status == ProfileStatus.PendingVerification)
        {
            Status = ProfileStatus.Verified;
            VerifiedAt ??= DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(adminId))
                VerifiedByAdminId = adminId;
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Suspend()
    {
        if (Status == ProfileStatus.Suspended)
            return Result.Failure("Profile is already suspended.");

        Status = ProfileStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProfileSuspendedDomainEvent(Id, UserId));

        return Result.Success();
    }

    public Result Reactivate()
    {
        if (Status != ProfileStatus.Suspended)
            return Result.Failure("Seul un profil suspendu peut être réactivé.");

        UpdatedAt = DateTime.UtcNow;

        if (Level >= Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified)
        {
            Status = ProfileStatus.Activate;
            AddDomainEvent(new ProfileActivatedDomainEvent(
                Id,
                ProfileTypeId,
                UserId,
                ProfileType?.CountryCode ?? string.Empty,
                ProfileType?.Currency ?? string.Empty,
                DateTime.UtcNow,
                Level));
        }
        else if (VerifiedAt.HasValue)
        {
            Status = ProfileStatus.Verified;
        }
        else
        {
            Status = ProfileStatus.PendingVerification;
        }

        return Result.Success();
    }
  
    public Result ChangeKind(ProfileKind NewKind)
    {
        var oldKind = Kind;
        Kind = NewKind;
        AddDomainEvent(new ProfileKindChangedDomainEvent(Id, UserId, oldKind, Kind));

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == ProfileStatus.Suspended)
            return Result.Failure("Ce profil est suspendu et ne peut pas être activé.");

        if (Status == ProfileStatus.Activate)
            return Result.Failure("Ce profil est déjà actif.");

        if (Status != ProfileStatus.Verified)
            return Result.Failure("Le profil doit d'abord être vérifié (L1 - Identité déclarée) avant de pouvoir être activé.");

        if (Level < Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified)
            return Result.Failure("Le KYC du profil doit être validé (L2 - Identité vérifiée minimum) conformément aux exigences du type de profil avant l'activation.");

        Status = ProfileStatus.Activate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProfileActivatedDomainEvent(
            Id,
            ProfileTypeId,
            UserId,
            ProfileType?.CountryCode ?? string.Empty,
            ProfileType?.Currency ?? string.Empty,
            DateTime.UtcNow,
            Level));

        return Result.Success();
    }

    public Result ApplyKycApproval(Shared.Domain.Enums.VerificationLevel approvedLevel = Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified)
    {
        var targetLevel = approvedLevel >= Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified
            ? approvedLevel
            : Shared.Domain.Enums.VerificationLevel.L2_IdentityVerified;

        if (targetLevel > Level)
            Level = targetLevel;

        VerifiedAt ??= DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (Status != ProfileStatus.Suspended)
        {
            Status = ProfileStatus.Activate;

            AddDomainEvent(new ProfileActivatedDomainEvent(
                Id,
                ProfileTypeId,
                UserId,
                ProfileType?.CountryCode ?? string.Empty,
                ProfileType?.Currency ?? string.Empty,
                DateTime.UtcNow,
                Level));
        }

        return Result.Success();
    }

    public bool IsVerified => Status == ProfileStatus.Verified || Status == ProfileStatus.Activate;
}
