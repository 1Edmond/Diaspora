using Document.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Document.Application.Queries;

public sealed record GetPendingDocumentsQuery(
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<DocumentDto>>>(PageNumber, PageSize);
