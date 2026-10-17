using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class UploadKYCDocumentCommandHandler(
    IKYCDbContext context,
    ILogger<UploadKYCDocumentCommandHandler> _logger,
    IFileStorageService fileStorage)
{
    private readonly IKYCDbContext _context = context;

    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<Result<Guid>> Handle(UploadKYCDocumentCommand command)
    {
        var verification = await _context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure<Guid>("Verification not found");

        if (command.File == null || command.File.Length <= 0 || command.File.Length > 10 * 1024 * 1024) // 10MB
            return Result.Failure<Guid>("File size must be greater than 0 and must not exceed 10MB");

        var safeFileName = Path.GetFileName(command.File.FileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
            return Result.Failure<Guid>("Invalid file name");

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Result.Failure<Guid>("Invalid file extension. Only PDF, JPG, JPEG, PNG, and WEBP are allowed");

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "application/pdf" };
        if (!allowedTypes.Contains(command.File.ContentType?.ToLowerInvariant() ?? string.Empty))
            return Result.Failure<Guid>("Invalid file type. Only JPG, PNG, WEBP, and PDF are allowed");

        if(_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Uploading encrypted document for verification {VerificationId}",
                command.VerificationId);


        using var stream = command.File.OpenReadStream();
        var fileName = $"kyc/{command.VerificationId}/{Guid.NewGuid()}_{safeFileName}";

        var uploadResult = await _fileStorage.UploadEncryptedFileAsync(
            stream,
            fileName,
            command.File.ContentType!,
            command.VerificationId.ToString(),
            "KYCDocument",                    
            CancellationToken.None);

        if (uploadResult.IsFailure)
            return Result.Failure<Guid>(uploadResult.Error);

        var result = verification.AddDocument(
            command.DocumentType,
            safeFileName,
            uploadResult.Value,
            command.File.Length);

        if (result.IsFailure)
        {
            await _fileStorage.DeleteFileAsync(uploadResult.Value);
            return Result.Failure<Guid>(result.Error);
        }

        await _context.SaveChangesAsync();

        var documentId = verification.Documents
            .OrderByDescending(d => d.UploadedAt)
            .First().Id;

        if(_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Successfully uploaded encrypted document {DocumentId} for verification {VerificationId}",
                documentId,
                command.VerificationId);

        return Result.Success(documentId);
    }
}

