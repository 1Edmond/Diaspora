using Shared.Domain.Primitives;
using System;

namespace Freelance.Domain.Entities;

public class WorkerVerificationStatus : AggregateRoot
{
    public Guid WorkerId { get; private set; }
    public bool IsVerified { get; private set; }
    public bool KycRequired { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationNotes { get; private set; }

    private WorkerVerificationStatus() { }

    public static WorkerVerificationStatus Create(Guid workerId, bool kycRequired = true)
    {
        return new WorkerVerificationStatus
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            IsVerified = false,
            KycRequired = kycRequired
        };
    }

    public void Verify(string? notes = null)
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerificationNotes = notes;
    }

    public void Unverify(string? notes = null)
    {
        IsVerified = false;
        VerifiedAt = null;
        VerificationNotes = notes;
    }

    public void SetKycRequired(bool required)
    {
        KycRequired = required;
    }
}