using KYC.Domain.Events;
using Shared.Domain.Attributes;
using Shared.Domain.Commons;
using Shared.Domain.Enums;
using Shared.Domain.Interfaces;
using Shared.Domain.Primitives;
using Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Entities;

public class KYCVerification : AggregateRoot, IAuditable, ISoftDeletable
{
    public Guid UserId { get; private set; }

    [SensitiveData]
    public EncryptedString FirstName { get; private set; } = null!;

    [SensitiveData]
    public EncryptedString LastName { get; private set; } = null!;

    [SensitiveData]
    public EncryptedString DateOfBirth { get; private set; } = null!;

    [SensitiveData]
    public EncryptedString Nationality { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public KYCStatus Status { get; private set; }
    public KYCLevel Level { get; private set; }

    public string? RejectionReason { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }

    private readonly List<KYCDocument> _documents = new();
    public IReadOnlyCollection<KYCDocument> Documents => _documents.AsReadOnly();

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }


    private KYCVerification() { }

    public static Result<KYCVerification> Create(
        Guid userId,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string nationality,
        Address address,
        string createdBy)
    {
        if (userId == Guid.Empty)
            return Result.Failure<KYCVerification>("User ID is required");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<KYCVerification>("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<KYCVerification>("Last name is required");

        if (dateOfBirth >= DateTime.UtcNow.AddYears(-18))
            return Result.Failure<KYCVerification>("User must be at least 18 years old");

        var verification = new KYCVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = EncryptedString.FromClear(firstName),
            LastName = EncryptedString.FromClear(lastName),
            DateOfBirth = EncryptedString.FromClear(dateOfBirth.ToString("yyyy-MM-dd")),
            Nationality = EncryptedString.FromClear(nationality),
            Address = address,
            Status = KYCStatus.Draft,
            Level = KYCLevel.None,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        verification.AddDomainEvent(new KYCVerificationCreatedEvent(verification.Id, userId));

        return Result.Success(verification);
    }

    public Result AddDocument(string documentType, string fileName, string filePath, long fileSize)
    {
        if (Status == KYCStatus.Approved)
            return Result.Failure("Cannot add documents to approved verification");

        var document = KYCDocument.Create(Id, documentType, fileName, filePath, fileSize);
        if (document.IsFailure)
            return Result.Failure(document.Error);

        _documents.Add(document.Value);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCDocumentAddedEvent(Id, UserId, document.Value.Id, documentType));

        return Result.Success();
    }

    public Result RemoveDocument(Guid documentId)
    {
        if (Status == KYCStatus.Approved)
            return Result.Failure("Cannot remove documents from approved verification");

        var document = _documents.FirstOrDefault(d => d.Id == documentId);
        if (document == null)
            return Result.Failure("Document not found");

        _documents.Remove(document);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCDocumentRemovedEvent(Id, UserId, documentId));

        return Result.Success();
    }

    public Result Submit(IEnumerable<string>? requiredDocumentTypes = null)
    {
        if (Status != KYCStatus.Draft)
            return Result.Failure("Only draft verifications can be submitted");

        if (!_documents.Any())
            return Result.Failure("At least one document is required");

        var requiredList = requiredDocumentTypes?.ToList();
        var requiredDocs = (requiredList != null && requiredList.Count > 0)
            ? requiredList
            : new List<string> { "IdentityDocument", "ProofOfAddress" };

        var missingDocs = requiredDocs.Where(type =>
            !_documents.Any(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase))).ToList();

        if (missingDocs.Count > 0)
            return Result.Failure($"Missing required documents: {string.Join(", ", missingDocs)}");

        Status = KYCStatus.Pending;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCVerificationSubmittedEvent(Id, UserId));

        return Result.Success();
    }

    public Result Approve(Guid reviewerId, KYCLevel level)
    {
        if (Status != KYCStatus.Pending && Status != KYCStatus.UnderReview)
            return Result.Failure("Only pending or under review verifications can be approved");

        Status = KYCStatus.Approved;
        Level = level;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
        RejectionReason = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCVerificationApprovedEvent(Id, UserId, level));

        return Result.Success();
    }

    public Result Reject(Guid reviewerId, string reason)
    {
        if (Status != KYCStatus.Pending && Status != KYCStatus.UnderReview)
            return Result.Failure("Only pending or under review verifications can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Rejection reason is required");

        Status = KYCStatus.Rejected;
        RejectionReason = reason;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCVerificationRejectedEvent(Id, UserId, reason));

        return Result.Success();
    }

    public Result RequestMoreInfo(Guid reviewerId, string message)
    {
        if (Status != KYCStatus.Pending && Status != KYCStatus.UnderReview)
            return Result.Failure("Invalid status for requesting more information");

        Status = KYCStatus.MoreInfoRequired;
        RejectionReason = message;
        ReviewedBy = reviewerId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KYCMoreInfoRequestedEvent(Id, UserId, message));

        return Result.Success();
    }

    public Result StartReview(Guid reviewerId)
    {
        if (Status != KYCStatus.Pending)
            return Result.Failure("Only pending verifications can be reviewed");

        Status = KYCStatus.UnderReview;
        ReviewedBy = reviewerId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}