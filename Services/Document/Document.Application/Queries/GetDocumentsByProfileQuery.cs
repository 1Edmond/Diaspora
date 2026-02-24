using Document.Application.DTOs;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Document.Application.Queries;

public sealed record GetDocumentsByProfileQuery(
    Guid ExternalProfileId,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<DocumentDto>>>(PageNumber, PageSize);