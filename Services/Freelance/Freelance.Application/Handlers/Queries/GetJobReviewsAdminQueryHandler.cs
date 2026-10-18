using Freelance.Application.Dtos;
using Freelance.Application.Interfaces;
using Freelance.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Freelance.Application.Handlers.Queries;

public class GetJobReviewsAdminQueryHandler(
    IJobReviewRepository _reviewRepository)
{
    public async Task<Result<PagedList<JobReviewDto>>> Handle(
        GetJobReviewsAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        var (reviews, totalCount) = await _reviewRepository.GetPagedReviewsAdminAsync(
            query.Page,
            query.PageSize,
            query.Rating,
            query.Search,
            cancellationToken);

        var items = reviews.Select(r => new JobReviewDto(
            r.Id,
            r.JobApplicationId,
            r.RaterId,
            r.RaterId.ToString(),
            r.SubjectId,
            r.SubjectId.ToString(),
            r.RaterRole,
            r.Rating,
            r.Punctuality,
            r.Quality,
            r.Communication,
            r.Comment,
            r.CreatedAt
        )).ToList();

        return Result.Success(new PagedList<JobReviewDto>(items, totalCount, query.Page, query.PageSize));
    }
}
