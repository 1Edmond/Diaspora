using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetListingReviewsQueryHandler(
    IApplicationDbContext _context,
    IProfileDirectoryRepository _profileDirectory)
{
    public async Task<Result<PagedList<ReviewDto>>> Handle(GetListingReviewsQuery query)
    {
        var q = _context.Reviews.Where(r => r.ListingId == query.ListingId);

        var totalCount = await q.CountAsync();

        var reviews = await q
            .OrderByDescending(r => r.IsVerified)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var reviewerIds = reviews.Select(r => r.ReviewerId).Distinct().ToList();
        var profiles = await _profileDirectory.GetByUserIdsAsync(reviewerIds);

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
                r.ProviderRepliedAt
            );
        }).ToList();

        return Result.Success(new PagedList<ReviewDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}