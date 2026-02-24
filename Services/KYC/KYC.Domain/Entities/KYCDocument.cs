using Shared.Domain.Commons;
using Shared.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Domain.Entities;

public class KYCDocument : Entity
{
    public Guid VerificationId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private KYCDocument() { }

    public static Result<KYCDocument> Create(
        Guid verificationId,
        string documentType,
        string fileName,
        string filePath,
        long fileSize)
    {
        var validTypes = new[] { "IdentityDocument", "ProofOfAddress", "Selfie", "Other" };
        if (!validTypes.Contains(documentType))
            return Result.Failure<KYCDocument>("Invalid document type");

        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<KYCDocument>("File name is required");

        if (fileSize <= 0 || fileSize > 10 * 1024 * 1024) // 10MB max
            return Result.Failure<KYCDocument>("File size must be between 1 byte and 10MB");

        var document = new KYCDocument
        {
            Id = Guid.NewGuid(),
            VerificationId = verificationId,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow
        };

        return Result.Success(document);
    }
}
