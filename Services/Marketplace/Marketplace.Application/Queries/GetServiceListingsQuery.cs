using Marketplace.Application.Dtos;
using Marketplace.Domain.Enums;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Queries;

public record GetServiceListingsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null,
    PaymentMode? PaymentMode = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool ActiveOnly = true
) : PagedQuery<Result<PagedList<ServiceListingSummaryDto>>>(PageNumber, PageSize);
