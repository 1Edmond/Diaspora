using Document.Application.Commands;
using Document.Application.Interfaces;
using Shared.Domain.Commons;
using Shared.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Handlers.Commands;

public sealed class ValidateDocumentCommandHandler(
    IDocumentRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> Handle(
        ValidateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(command.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure("Document not found.");

        var result = document.Validate(command.AdminId);
        if (result.IsFailure)
            return result;

        await repository.UpdateAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
