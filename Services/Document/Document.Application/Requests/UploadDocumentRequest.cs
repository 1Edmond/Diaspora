using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Requests;

public sealed record UploadDocumentRequest(
    Guid ExternalProfileId,
    Guid DocumentTypeId,
    IFormFile File,
    DateTime? ExpiresAt,
    DateTime? IssuedAt,
    string? IssuedBy
);