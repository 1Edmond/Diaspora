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
            phoneNumber.GetFullNumber()));

        return Result.Success(profile);
    }

    /// <summary>
    /// Called by an admin to validate the user's Togo phone number and identity.
    /// Transitions status from PendingVerification → Verified.W
    /// </summary>
    public Result Verify(string adminId)
    {
        if (Status != ProfileStatus.PendingVerification)
            return Result.Failure("Only profiles pending verification can be verified.");

        if (string.IsNullOrWhiteSpace(adminId))
            return Result.Failure("Admin ID is required.");

        Status = ProfileStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerifiedByAdminId = adminId;

        AddDomainEvent(new ProfileVerifiedDomainEvent(
            Id, UserId, adminId, VerifiedAt.Value));

        return Result.Success();
    }

    public Result Suspend()
    {
        if (Status == ProfileStatus.Suspended)
            return Result.Failure("Profile is already suspended.");

        Status = ProfileStatus.Suspended;
        AddDomainEvent(new ProfileSuspendedDomainEvent(Id, UserId));

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
            return Result.Failure("Profil suspendu, impossible d'activer.");

        if (Status == ProfileStatus.Activate)
            return Result.Failure("Profile is already activated.");

        Status = ProfileStatus.Activate;
        AddDomainEvent(new ProfileActivatedDomainEvent(
            Id,
            ProfileTypeId,
            UserId,
            ProfileType?.CountryCode ?? string.Empty,
            ProfileType?.Currency ?? string.Empty,
            DateTime.UtcNow));

        return Result.Success();
    }

    public bool IsVerified => Status == ProfileStatus.Verified;
}
