using Document.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.DTOs;

public sealed record DocumentDto(
    Guid Id,
    Guid ProfileId,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string DocumentTypeCode,
    DocumentStatus Status,
    string FileName,
    long FileSize,
    string MimeType,
    string FilePath,
    DateTime? ExpiresAt,
    DateTime? IssuedAt,
    string? IssuedBy,
    string? RejectionReason,
    DateTime? ValidatedAt,
    DateTime CreatedAt
);
