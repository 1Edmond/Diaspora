using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Handlers.Queries;

public class GetReviewsAdminQueryHandler(
    IReviewRepository _reviewRepository,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ReviewDto>>> Handle(
        GetReviewsAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        var (reviews, totalCount) = await _reviewRepository.GetPagedReviewsAdminAsync(
            query.PageNumber,
            query.PageSize,
            query.Rating,
            query.Search,
            cancellationToken);

        var reviewerIds = reviews.Select(r => r.ReviewerId).Distinct().ToList();
        var profiles = await _profileDirectory.GetByUserIdsAsync(reviewerIds, cancellationToken);

        var items = reviews.Select(r =>
        {
            profiles.TryGetValue(r.ReviewerId, out var profile);
            return new ReviewDto(
                r.Id,
                r.ListingId,
                r.ReviewerId,
                profile?.Id,
                profile?.DisplayName ?? string.Empty,
                profile?.IsVerified ?? false,
                r.Rating,
                r.Comment,
                r.IsVerified,
                r.CreatedAt,
                r.ProviderReply,
                r.ProviderRepliedAt,
                r.Listing?.Title
            );
        }).ToList();

        return Result.Success(new PagedList<ReviewDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
