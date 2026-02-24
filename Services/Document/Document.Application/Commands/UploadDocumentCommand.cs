using Document.Application.Responses;
using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Commands;

public sealed record UploadDocumentCommand(
    Guid ExternalProfileId,
    Guid DocumentTypeId,
    IFormFile File,
    DateTime? ExpiresAt,
    DateTime? IssuedAt,
    string? IssuedBy
) : ICommand<Result<UploadDocumentResponse>>;