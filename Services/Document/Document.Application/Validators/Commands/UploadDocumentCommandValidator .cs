using Document.Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Validators.Commands;

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedMimeTypes =
        ["application/pdf", "image/jpeg", "image/png", "image/webp"];

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.DocumentTypeId).NotEmpty();

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(f => f.Length > 0).WithMessage("File cannot be empty.")
            .Must(f => f.Length <= MaxFileSizeBytes)
                .WithMessage("File size must not exceed 10 MB.")
            .Must(f => AllowedMimeTypes.Contains(f.ContentType))
                .WithMessage("Only PDF, JPEG, PNG and WebP files are allowed.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future.");
    }
}