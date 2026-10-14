using Document.Application.Commands;
using Document.Application.Interfaces;
using Document.Application.Responses;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Handlers.Commands;

public sealed class UploadDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IDocumentTypeRepository documentTypeRepository,
    IFileStorageService fileStorageService,
    IDocumentAccessService documentAccessService,
    IUnitOfWork unitOfWork)
{

    public const string FileEntityType = "document-file";

    public async Task<Result<UploadDocumentResponse>> Handle(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        if (!await documentAccessService.CanAccessProfileAsync(command.ProfileId, cancellationToken))
            return Result.Failure<UploadDocumentResponse>("Access denied to this profile.");

        var documentType = await documentTypeRepository.GetByIdAsync(
            command.DocumentTypeId, cancellationToken);

        if (documentType is null)
            return Result.Failure<UploadDocumentResponse>("Document type not found.");

        if (!documentType.IsActive)
            return Result.Failure<UploadDocumentResponse>("Document type is not active.");

        await using var stream = command.File.OpenReadStream();
        var uploadResult = await fileStorageService.UploadEncryptedFileAsync(
             stream,
             command.File.FileName,
             command.File.ContentType,
             entityId: command.ProfileId.ToString(),
             entityType: FileEntityType,
             cancellationToken);

        if (uploadResult.IsFailure)
            return Result.Failure<UploadDocumentResponse>(uploadResult.Error);

        var documentResult = Domain.Entities.Document.Create(
                    command.ProfileId,
                    documentType,
                    filePath: uploadResult.Value,
                    fileName: command.File.FileName,
                    fileSize: command.File.Length,
                    mimeType: command.File.ContentType,
                    command.ExpiresAt,
                    command.IssuedAt,
                    command.IssuedBy);

        if (documentResult.IsFailure)
        {
            await fileStorageService.DeleteFileAsync(uploadResult.Value, cancellationToken);
            return Result.Failure<UploadDocumentResponse>(documentResult.Error);
        }

        await documentRepository.AddAsync(documentResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UploadDocumentResponse(
            documentResult.Value.Id,
            documentResult.Value.FileName));
    }
}
