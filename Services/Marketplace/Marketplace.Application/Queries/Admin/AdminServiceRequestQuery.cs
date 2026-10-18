using Marketplace.Application.Dtos;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;

namespace Marketplace.Application.Queries.Admin;

public record AdminServiceRequestQuery(
    int PageNumber = 1,
    int PageSize = 10,
    ServiceRequestStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    Guid? ProviderId = null,
    Guid? RequesterId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Search = null
) : PagedQuery<Result<PagedList<ServiceRequestDto>>>(PageNumber, PageSize);
