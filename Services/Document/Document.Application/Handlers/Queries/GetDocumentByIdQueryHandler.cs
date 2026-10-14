using Document.Application.DTOs;
using Document.Application.Extensions;
using Document.Application.Interfaces;
using Document.Application.Queries;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Handlers.Queries;

public sealed class GetDocumentByIdQueryHandler(
    IDocumentRepository repository,
    IDocumentAccessService documentAccessService)
{
    public async Task<Result<DocumentDto>> Handle(
        GetDocumentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(query.DocumentId, cancellationToken);
        if (document is null || !await documentAccessService.CanAccessDocumentAsync(document, cancellationToken))
            return Result.Failure<DocumentDto>("Document not found.");

        return Result.Success(document.ToDto());
    }
}