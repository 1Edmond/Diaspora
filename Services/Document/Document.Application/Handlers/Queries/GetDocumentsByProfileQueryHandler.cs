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

public sealed class GetDocumentsByProfileQueryHandler(IDocumentRepository repository)
{
    public async Task<Result<PagedList<DocumentDto>>> Handle(
        GetDocumentsByProfileQuery query,
        CancellationToken cancellationToken)
    {
        var documents = await repository.GetByExternalProfileIdAsync(
            query.ExternalProfileId, cancellationToken);

        var dtos = documents.Select(d => d.ToDto());
        var paged = PagedList<DocumentDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}

