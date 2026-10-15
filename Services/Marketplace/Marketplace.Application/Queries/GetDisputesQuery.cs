using Marketplace.Application.DTOs;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Marketplace.Application.Queries;

public sealed record GetDisputesQuery(
    DisputeStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20
) : PagedQuery<Result<PagedList<DisputeDto>>>(PageNumber, PageSize);
