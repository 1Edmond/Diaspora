using Marketplace.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Marketplace.Application.Queries;

public record GetReviewsAdminQuery(
    int PageNumber = 1,
    int PageSize = 20,
    int? Rating = null,
    string? Search = null) : PagedQuery<Result<PagedList<ReviewDto>>>(PageNumber, PageSize);
