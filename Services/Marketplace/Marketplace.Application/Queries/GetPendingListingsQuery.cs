using Marketplace.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Queries;

public record GetPendingListingsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : PagedQuery<Result<PagedList<ServiceListingSummaryDto>>>(PageNumber, PageSize);
