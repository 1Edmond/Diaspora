using Document.Application.Commands;
using Document.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Handlers.Commands;

public sealed class DeleteDocumentCommandHandler(
    IDocumentRepository repository,
    IFileStorageService fileStorageService,
    IDocumentAccessService documentAccessService,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        DeleteDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(command.DocumentId, cancellationToken);
        if (document is null || !await documentAccessService.CanAccessDocumentAsync(document, cancellationToken))
            return Result.Failure("Document not found.");

        var result = document.Delete();
        if (result.IsFailure)
            return result;

        // Suppression physique du fichier
        await fileStorageService.DeleteFileAsync(document.FilePath, cancellationToken);

        await repository.DeleteAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
