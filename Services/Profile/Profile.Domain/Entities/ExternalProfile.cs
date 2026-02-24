using Profile.Domain.Enums;
using Profile.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Profile.Domain.Entities;

public sealed class ExternalProfile : AggregateRoot
{
    public Guid InternalProfileId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ExternalProfileTypeId { get; private set; }

    public ExternalProfileKind Kind { get; private set; }
    public ExternalProfileStatus Status { get; private set; }
    public Guid? ArrivalKycVerificationId { get; private set; }
    public DateTime? ArrivalValidatedAt { get; private set; }

    public bool CanOfferServices => Status == ExternalProfileStatus.Active;

    public ExternalProfileType ExternalProfileType { get; private set; } = null!;

    private ExternalProfile() { }

    private ExternalProfile(
        Guid id,
        Guid internalProfileId,
        Guid userId,
        Guid externalProfileTypeId,
        ExternalProfileKind kind) : base(id)
    {
        InternalProfileId = internalProfileId;
        UserId = userId;
        ExternalProfileTypeId = externalProfileTypeId;
        Kind = kind;
        Status = ExternalProfileStatus.PendingArrival;
    }


    public static Result<ExternalProfile> Create(
        InternalProfile internalProfile,
        Guid externalProfileTypeId,
        ExternalProfileKind kind)
    {
        if (!internalProfile.IsVerified)
            return Result.Failure<ExternalProfile>(
                "Internal profile must be verified before creating an external profile.");

        if (externalProfileTypeId == Guid.Empty)
            return Result.Failure<ExternalProfile>("ExternalProfileType is required.");

        var profile = new ExternalProfile(
            Guid.NewGuid(),
            internalProfile.Id,
            internalProfile.UserId,
            externalProfileTypeId,
            kind);

        profile.AddDomainEvent(new ExternalProfileCreatedDomainEvent(
            profile.Id,
            internalProfile.Id,
            internalProfile.UserId,
            externalProfileTypeId,
            kind));

        return Result.Success(profile);
    }

    public Result Activate(Guid kycVerificationId, string countryCode)
    {
        if (Status != ExternalProfileStatus.PendingArrival)
            return Result.Failure("Only profiles pending arrival can be activated.");

        if (kycVerificationId == Guid.Empty)
            return Result.Failure("KYC verification ID is required.");

        Status = ExternalProfileStatus.Active;
        ArrivalKycVerificationId = kycVerificationId;
        ArrivalValidatedAt = DateTime.UtcNow;

        AddDomainEvent(new ExternalProfileActivatedDomainEvent(
            Id, UserId, countryCode, ArrivalValidatedAt.Value));

        return Result.Success();
    }

    public Result ChangeKind(ExternalProfileKind newKind)
    {
        if (Status != ExternalProfileStatus.Active)
            return Result.Failure("Profile must be active to change kind.");

        if (Kind == newKind)
            return Result.Failure($"Profile is already of kind {newKind}.");

        var oldKind = Kind;
        Kind = newKind;

        AddDomainEvent(new ExternalProfileKindChangedDomainEvent(
            Id, UserId, oldKind, newKind));

        return Result.Success();
    }

    public Result Suspend()
    {
        if (Status == ExternalProfileStatus.Suspended)
            return Result.Failure("Profile is already suspended.");

        Status = ExternalProfileStatus.Suspended;
        AddDomainEvent(new ExternalProfileSuspendedDomainEvent(Id, UserId));

        return Result.Success();
    }
}

