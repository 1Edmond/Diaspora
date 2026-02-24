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

public sealed class GetExpiringDocumentsQueryHandler(IDocumentRepository repository)
{
    public async Task<Result<PagedList<DocumentDto>>> Handle(
        GetExpiringDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var threshold = DateTime.UtcNow.AddDays(query.DaysAhead);

        var documents = await repository.GetExpiringDocumentsAsync(threshold, cancellationToken);

        var dtos = documents.Select(d => d.ToDto());
        var paged = PagedList<DocumentDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}