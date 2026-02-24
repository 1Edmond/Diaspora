using Procedure.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Procedure.Domain.Entities;

public sealed class UserProcedure : AggregateRoot
{
    /// <summary>
    /// InternalProfileId si procédure Internal, ExternalProfileId si External.
    /// </summary>
    public Guid ProfileId { get; private set; }
    public Guid ProcedureId { get; private set; }
    public UserProcedureStatus Status { get; private set; }
    public string? CompletedByUserId { get; private set; }
    public string? CompletedByRole { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    // Navigation EF Core
    public Procedure Procedure { get; private set; } = null!;

    private UserProcedure() { }

    private UserProcedure(Guid id, Guid profileId, Guid procedureId) : base(id)
    {
        ProfileId = profileId;
        ProcedureId = procedureId;
        Status = UserProcedureStatus.NotStarted;
    }

    public static Result<UserProcedure> Create(Guid profileId, Guid procedureId)
    {
        if (profileId == Guid.Empty)
            return Result.Failure<UserProcedure>("ProfileId is required.");

        if (procedureId == Guid.Empty)
            return Result.Failure<UserProcedure>("ProcedureId is required.");

        var userProcedure = new UserProcedure(Guid.NewGuid(), profileId, procedureId);

        userProcedure.AddDomainEvent(new UserProcedureStartedDomainEvent(
            userProcedure.Id,
            profileId,
            procedureId,
            string.Empty));

        return Result.Success(userProcedure);
    }

    public Result Complete(
        string completedByUserId,
        string role,
        string? notes,
        bool dependenciesBlocking,
        IEnumerable<UserProcedureStatus> dependencyStatuses)
    {
        if (Status == UserProcedureStatus.Completed)
            return Result.Failure("Procedure is already completed.");

        if (dependenciesBlocking)
        {
            var unmet = dependencyStatuses
                .Any(s => s != UserProcedureStatus.Completed && s != UserProcedureStatus.Skipped);

            if (unmet)
                return Result.Failure(
                    "All dependent procedures must be completed or skipped first.");
        }

        Status = UserProcedureStatus.Completed;
        CompletedByUserId = completedByUserId;
        CompletedByRole = role;
        CompletedAt = DateTime.UtcNow;
        Notes = notes;

        AddDomainEvent(new UserProcedureCompletedDomainEvent(
            Id, ProfileId, ProcedureId,
            Procedure?.Title ?? string.Empty,
            completedByUserId, role.ToString()));

        return Result.Success();
    }

    public Result Start()
    {
        if (Status != UserProcedureStatus.NotStarted)
            return Result.Failure("Procedure has already been started.");

        Status = UserProcedureStatus.InProgress;

        AddDomainEvent(new UserProcedureStartedDomainEvent(
            Id, ProfileId, ProcedureId,
            Procedure?.Title ?? string.Empty));

        return Result.Success();
    }

    public Result Skip(string? notes)
    {
        if (Status == UserProcedureStatus.Completed)
            return Result.Failure("Cannot skip a completed procedure.");

        Status = UserProcedureStatus.Skipped;
        Notes = notes;

        AddDomainEvent(new UserProcedureSkippedDomainEvent(
            Id, ProfileId, ProcedureId,
            Procedure?.Title ?? string.Empty));

        return Result.Success();
    }

    public void UpdateNotes(string? notes) => Notes = notes;
}
