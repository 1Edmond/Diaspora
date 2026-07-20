using Marketplace.Application.Abstractions;
using Marketplace.Application.Dtos;
using Marketplace.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace.Application.Handlers.Queries;

public class GetListingReviewsQueryHandler(IApplicationDbContext _context)
{
    public async Task<Result<PagedList<ReviewDto>>> Handle(GetListingReviewsQuery query)
    {
        var q = _context.Reviews.Where(r => r.ListingId == query.ListingId);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderByDescending(r => r.IsVerified)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReviewDto(
                r.Id, r.ListingId, r.ReviewerId,
                string.Empty, // Résolu via Profile Service
                r.Rating, r.Comment, r.IsVerified, r.CreatedAt
            ))
            .ToListAsync();

        return Result.Success(new PagedList<ReviewDto>(items, totalCount, query.PageNumber, query.PageSize));
    }
}