using Document.Application.DTOs;
using Document.Application.Extensions;
using Document.Application.Interfaces;
using Document.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Handlers.Queries;

public sealed class GetDocumentsByProfileQueryHandler(
    IDocumentRepository repository,
    IDocumentAccessService documentAccessService)
{
    public async Task<Result<PagedList<DocumentDto>>> Handle(
        GetDocumentsByProfileQuery query,
        CancellationToken cancellationToken)
    {
        if (!await documentAccessService.CanAccessProfileAsync(query.ExternalProfileId, cancellationToken))
            return Result.Failure<PagedList<DocumentDto>>("Profile not found.");

        var documents = await repository.GetByExternalProfileIdAsync(
            query.ExternalProfileId, cancellationToken);

        var dtos = documents.Select(d => d.ToDto());
        var paged = PagedList<DocumentDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}

