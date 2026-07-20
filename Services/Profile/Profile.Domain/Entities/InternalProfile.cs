using Profile.Domain.Enums;
using Profile.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class InternalProfile : AggregateRoot
{
    public Guid UserId { get; private set; }

    public Guid InternalProfileTypeId { get; private set; }
    public InternalProfileType InternalProfileType { get; private set; } = null!;

    // Phone number with Togo indicatif — stored as EncryptedString in infra
    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }

    public InternalProfileStatus Status { get; private set; }

    public DateTime? VerifiedAt { get; private set; }
    public string? VerifiedByAdminId { get; private set; }

    // Navigation — EF Core
    private readonly List<ExternalProfile> _externalProfiles = new();
    public IReadOnlyCollection<ExternalProfile> ExternalProfiles => _externalProfiles.AsReadOnly();

    private InternalProfile() { }

    private InternalProfile(
        Guid id,
        Guid userId,
        Guid internalProfileTypeId,
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
        InternalProfileTypeId = internalProfileTypeId;
        Status = InternalProfileStatus.PendingVerification;
    }

    public static Result<InternalProfile> Create(
        Guid userId,
        Guid internalProfileTypeId,
        PhoneNumber phoneNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        if (userId == Guid.Empty)
            return Result.Failure<InternalProfile>("UserId is required.");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<InternalProfile>("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<InternalProfile>("Last name is required.");

        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-15)))
            return Result.Failure<InternalProfile>("User must be at least 15 years old.");



        var profile = new InternalProfile(
            Guid.NewGuid(),
            userId,
            internalProfileTypeId,
            phoneNumber,
            firstName.Trim(),
            lastName.Trim(),
            dateOfBirth);

        profile.AddDomainEvent(new InternalProfileCreatedDomainEvent(
            profile.Id,
            userId,
            phoneNumber.GetFullNumber()));

        return Result.Success(profile);
    }

    /// <summary>
    /// Called by an admin to validate the user's Togo phone number and identity.
    /// Transitions status from PendingVerification → Verified.
    /// </summary>
    public Result Verify(string adminId)
    {
        if (Status != InternalProfileStatus.PendingVerification)
            return Result.Failure("Only profiles pending verification can be verified.");

        if (string.IsNullOrWhiteSpace(adminId))
            return Result.Failure("Admin ID is required.");

        Status = InternalProfileStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerifiedByAdminId = adminId;

        AddDomainEvent(new InternalProfileVerifiedDomainEvent(
            Id, UserId, adminId, VerifiedAt.Value));

        return Result.Success();
    }

    public Result Suspend()
    {
        if (Status == InternalProfileStatus.Suspended)
            return Result.Failure("Profile is already suspended.");

        Status = InternalProfileStatus.Suspended;
        AddDomainEvent(new InternalProfileSuspendedDomainEvent(Id, UserId));

        return Result.Success();
    }

    public bool IsVerified => Status == InternalProfileStatus.Verified;
}
