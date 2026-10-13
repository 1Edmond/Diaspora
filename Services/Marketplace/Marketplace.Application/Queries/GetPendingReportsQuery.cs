using Marketplace.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Queries;

public record GetPendingReportsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : PagedQuery<Result<PagedList<ReportDto>>>(PageNumber, PageSize);