using Document.Domain.Enums;
using Document.Domain.Events;
using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Document.Domain.Entities;

public sealed class Document : AggregateRoot
{
    public Guid ProfileId { get; private set; }
    public Guid DocumentTypeId { get; private set; }
    public DocumentStatus Status { get; private set; }

    // Fichier
    public string FilePath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string MimeType { get; private set; } = string.Empty;

    // Métadonnées
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public string? IssuedBy { get; private set; }

    // Validation
    public string? RejectionReason { get; private set; }
    public string? ValidatedByAdminId { get; private set; }
    public DateTime? ValidatedAt { get; private set; }

    // Alertes
    public DateTime? RenewalAlertSentAt { get; private set; }

    // Navigation EF Core
    public DocumentType DocumentType { get; private set; } = null!;

    private Document() { }

    private Document(
        Guid id,
        Guid externalProfileId,
        Guid documentTypeId,
        string filePath,
        string fileName,
        long fileSize,
        string mimeType,
        DateTime? expiresAt,
        DateTime? issuedAt,
        string? issuedBy) : base(id)
    {
        ProfileId = externalProfileId;
        DocumentTypeId = documentTypeId;
        Status = DocumentStatus.Pending;
        FilePath = filePath;
        FileName = fileName;
        FileSize = fileSize;
        MimeType = mimeType;
        ExpiresAt = expiresAt;
        IssuedAt = issuedAt;
        IssuedBy = issuedBy;
    }

    public static Result<Document> Create(
        Guid externalProfileId,
        DocumentType documentType,
        string filePath,
        string fileName,
        long fileSize,
        string mimeType,
        DateTime? expiresAt,
        DateTime? issuedAt,
        string? issuedBy)
    {
        if (externalProfileId == Guid.Empty)
            return Result.Failure<Document>("ExternalProfileId is required.");

        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Failure<Document>("File path is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<Document>("File name is required.");

        if (fileSize <= 0)
            return Result.Failure<Document>("File size must be greater than 0.");

        if (documentType.IsRenewable && expiresAt is null)
            return Result.Failure<Document>(
                $"Expiration date is required for renewable document type '{documentType.Name}'.");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            return Result.Failure<Document>("Expiration date must be in the future.");

        var document = new Document(
            Guid.NewGuid(),
            externalProfileId,
            documentType.Id,
            filePath,
            fileName,
            fileSize,
            mimeType,
            expiresAt,
            issuedAt,
            issuedBy);

        document.AddDomainEvent(new DocumentUploadedDomainEvent(
            document.Id,
            externalProfileId,
            documentType.Id,
            documentType.Name,
            fileName));

        return Result.Success(document);
    }

    public Result Validate(string adminId)
    {
        if (Status != DocumentStatus.Pending)
            return Result.Failure("Only pending documents can be validated.");

        Status = DocumentStatus.Active;
        ValidatedByAdminId = adminId;
        ValidatedAt = DateTime.UtcNow;

        AddDomainEvent(new DocumentValidatedDomainEvent(
            Id,
            ProfileId,
            DocumentType?.Name ?? string.Empty,
            ExpiresAt));

        return Result.Success();
    }

    public Result Reject(string adminId, string reason)
    {
        if (Status != DocumentStatus.Pending)
            return Result.Failure("Only pending documents can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Rejection reason is required.");

        Status = DocumentStatus.Rejected;
        RejectionReason = reason;
        ValidatedByAdminId = adminId;
        ValidatedAt = DateTime.UtcNow;

        AddDomainEvent(new DocumentRejectedDomainEvent(
            Id,
            ProfileId,
            DocumentType?.Name ?? string.Empty,
            reason));

        return Result.Success();
    }

    /// <summary>
    /// Appelé par le DocumentExpiryCheckerService (BackgroundService).
    /// </summary>
    public Result MarkAsExpired()
    {
        if (Status != DocumentStatus.Active)
            return Result.Failure("Only active documents can expire.");

        Status = DocumentStatus.Expired;

        AddDomainEvent(new DocumentExpiredDomainEvent(
            Id,
            ProfileId,
            DocumentType?.Name ?? string.Empty,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Appelé par le BackgroundService pour envoyer l'alerte de renouvellement.
    /// Garantit qu'une seule alerte est envoyée par cycle.
    /// </summary>
    public Result SendRenewalAlert(int daysRemaining)
    {
        if (Status != DocumentStatus.Active)
            return Result.Failure("Only active documents can trigger renewal alerts.");

        if (!ExpiresAt.HasValue)
            return Result.Failure("Document has no expiration date.");

        RenewalAlertSentAt = DateTime.UtcNow;

        AddDomainEvent(new DocumentRenewalAlertDomainEvent(
            Id,
            ProfileId,
            DocumentType?.Name ?? string.Empty,
            ExpiresAt.Value,
            daysRemaining));

        return Result.Success();
    }

    public Result Delete()
    {
        if (Status == DocumentStatus.Active)
            return Result.Failure("Active documents cannot be deleted. Reject it first.");

        AddDomainEvent(new DocumentDeletedDomainEvent(Id, ProfileId));

        return Result.Success();
    }
}
