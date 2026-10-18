using Freelance.Application.Dtos;
using Shared.Application.Models;
using Shared.Domain.Commons;

namespace Freelance.Application.Queries;

public record GetJobReviewsAdminQuery(
    int Page = 1,
    int PageSize = 20,
    int? Rating = null,
    string? Search = null) : PagedQuery<Result<PagedList<JobReviewDto>>>(Page, PageSize);
