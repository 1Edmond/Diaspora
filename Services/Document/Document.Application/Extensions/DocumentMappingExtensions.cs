using Document.Application.DTOs;
using Document.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Extensions;

public static class DocumentMappingExtensions
{
    public static DocumentDto ToDto(this Domain.Entities.Document doc) =>
         new(
             doc.Id,
             doc.ProfileId,
             doc.DocumentTypeId,
             doc.DocumentType?.Name ?? string.Empty,
             doc.DocumentType?.Code ?? string.Empty,
             doc.Status,
             doc.FileName,
             doc.FileSize,
             doc.MimeType,
             doc.FilePath,
             doc.ExpiresAt,
             doc.IssuedAt,
             doc.IssuedBy,
             doc.RejectionReason,
             doc.ValidatedAt,
             doc.CreatedAt);

    public static DocumentTypeDto ToDto(this DocumentType type) =>
        new(
            type.Id,
            type.Name,
            type.Code,
            type.Description,
            type.IsRenewable,
            type.RenewalAlertDays,
            type.IsRequired,
            type.IsActive);
}
