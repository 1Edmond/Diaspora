using Freelance.Domain.Enums;
using Freelance.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

public class JobApplication : AggregateRoot
{
    public Guid JobPostingId { get; private set; }
    public Guid WorkerId { get; private set; }
    public string? Message { get; private set; }
    public JobApplicationStatus Status { get; private set; }

    public DateTime? DecidedAt { get; private set; }
    public Guid? DecidedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    public Guid? ChatThreadId { get; private set; }
    public Guid? EscrowTransactionId { get; private set; }

    public JobPosting Posting { get; private set; } = null!;

    private JobApplication() { }

    public static JobApplication Create(Guid jobPostingId, Guid workerId, string? message, Guid employerId)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobPostingId = jobPostingId,
            WorkerId = workerId,
            Message = message,
            Status = JobApplicationStatus.Pending
        };

        application.AddDomainEvent(new JobApplicationCreatedEvent(application.Id, jobPostingId, workerId, employerId));

        return application;
    }

    public Result Accept(Guid decidedBy, Guid? escrowTransactionId)
    {
        if (Status != JobApplicationStatus.Pending)
            return Result.Failure("Only a Pending application can be accepted.");

        Status = JobApplicationStatus.Accepted;
        DecidedAt = DateTime.UtcNow;
        DecidedBy = decidedBy;
        EscrowTransactionId = escrowTransactionId;

        AddDomainEvent(new JobApplicationAcceptedEvent(Id, JobPostingId, WorkerId, decidedBy));

        return Result.Success();
    }

    public Result Reject(Guid decidedBy, string? reason)
    {
        if (Status != JobApplicationStatus.Pending)
            return Result.Failure("Only a Pending application can be rejected.");

        Status = JobApplicationStatus.Rejected;
        DecidedAt = DateTime.UtcNow;
        DecidedBy = decidedBy;
        RejectionReason = reason;

        AddDomainEvent(new JobApplicationRejectedEvent(Id, JobPostingId, WorkerId, decidedBy, reason));

        return Result.Success();
    }

    public Result Withdraw()
    {
        if (Status is not (JobApplicationStatus.Pending or JobApplicationStatus.Accepted))
            return Result.Failure("Only a Pending or Accepted application can be withdrawn.");

        Status = JobApplicationStatus.Withdrawn;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != JobApplicationStatus.Accepted)
            return Result.Failure("Only an Accepted application can be completed.");

        Status = JobApplicationStatus.Completed;

        AddDomainEvent(new JobApplicationCompletedEvent(Id, JobPostingId, WorkerId, DecidedBy ?? Guid.Empty));

        return Result.Success();
    }

    public Result MarkNoShow(Guid decidedBy)
    {
        if (Status != JobApplicationStatus.Accepted)
            return Result.Failure("Only an Accepted application can be marked as NoShow.");

        Status = JobApplicationStatus.NoShow;
        DecidedBy = decidedBy;
        DecidedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void LinkChatThread(Guid chatThreadId) => ChatThreadId = chatThreadId;
}