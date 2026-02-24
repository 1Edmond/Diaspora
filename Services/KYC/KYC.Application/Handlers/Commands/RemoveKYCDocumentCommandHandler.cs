using KYC.Application.Abstractions;
using KYC.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace KYC.Application.Handlers.Commands;

public class RemoveKYCDocumentCommandHandler(
    IKYCDbContext _context,
    IFileStorageService _fileStorage)
{
    public async Task<Result> Handle(RemoveKYCDocumentCommand command)
    {
        var verification = await _context.KYCVerifications
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == command.VerificationId);

        if (verification == null)
            return Result.Failure("Verification not found");

        var document = verification.Documents
            .FirstOrDefault(d => d.Id == command.DocumentId);

        if (document == null)
            return Result.Failure("Document not found");

        await _fileStorage.DeleteFileAsync(document.FilePath);

        var result = verification.RemoveDocument(command.DocumentId);

        if (result.IsFailure)
            return result;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}