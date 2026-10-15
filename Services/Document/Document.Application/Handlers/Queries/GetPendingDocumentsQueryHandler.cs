using Document.Application.DTOs;
using Document.Application.Extensions;
using Document.Application.Interfaces;
using Document.Application.Queries;
using Document.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Document.Application.Handlers.Queries;

public sealed class GetPendingDocumentsQueryHandler(IDocumentRepository repository)
{
    public async Task<Result<PagedList<DocumentDto>>> Handle(
        GetPendingDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var documents = await repository.GetByStatusAsync(DocumentStatus.Pending, cancellationToken);

        var dtos = documents.Select(d => d.ToDto());
        var paged = PagedList<DocumentDto>.Create(dtos, query.PageNumber, query.PageSize);

        return Result.Success(paged);
    }
}
