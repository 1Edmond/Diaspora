using Microsoft.AspNetCore.Http;
using Shared.Application.Abstractions;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Commands;

public record UploadKYCDocumentCommand(
    Guid VerificationId,
    string DocumentType,
    IFormFile File
) : ICommand<Result<Guid>>;

